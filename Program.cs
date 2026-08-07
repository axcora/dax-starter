using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var cmd = args.Length > 0? args[0] : "build";
            if (cmd == "init") { InitProject(); return; }
            if (cmd == "build") { new DaxBuilder().Build(); return; }
            if (cmd == "start") { new DaxBuilder().Build(); new DaxServer().Start(); return; }
            Console.WriteLine("Usage: dax build | dax start | dax init");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[FATAL] " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
    static void InitProject()
    {
        Console.WriteLine("[DAX] init...");
        Directory.CreateDirectory("_data");
        Directory.CreateDirectory("content/posts");
        Directory.CreateDirectory("templates/layouts");
        Directory.CreateDirectory("templates/partials");
        Directory.CreateDirectory("public/css");
        Directory.CreateDirectory("site");
        if (!File.Exists("_data/metadata.json")) File.WriteAllText("_data/metadata.json", Sample.Metadata);
        if (!File.Exists("_data/config.yaml")) File.WriteAllText("_data/config.yaml", Sample.ConfigYaml);
        if (!File.Exists("_data/nav.json")) File.WriteAllText("_data/nav.json", "{\"nav1_name\":\"Home\",\"nav1_url\":\"/\"}");
        if (!File.Exists("content/index.md")) File.WriteAllText("content/index.md", Sample.IndexMd);
        if (!File.Exists("content/tags.md")) File.WriteAllText("content/tags.md", Sample.TagsMd);
        if (!File.Exists("content/posts.md")) File.WriteAllText("content/posts.md", Sample.PostsMd);
        if (!File.Exists("content/posts/hello.md")) File.WriteAllText("content/posts/hello.md", Sample.Post1);
        if (!File.Exists("templates/layouts/base.dax")) File.WriteAllText("templates/layouts/base.dax", Sample.BaseDax);
        if (!File.Exists("templates/layouts/home.dax")) File.WriteAllText("templates/layouts/home.dax", Sample.HomeDax);
        if (!File.Exists("templates/layouts/posts-list.dax")) File.WriteAllText("templates/layouts/posts-list.dax", Sample.ListDax);
        if (!File.Exists("templates/layouts/post.dax")) File.WriteAllText("templates/layouts/post.dax", Sample.PostDax);
        if (!File.Exists("templates/layouts/tag.dax")) File.WriteAllText("templates/layouts/tag.dax", Sample.TagDax);
        if (!File.Exists("templates/layouts/tags-list.dax")) File.WriteAllText("templates/layouts/tags-list.dax", Sample.TagsListDax);
        if (!File.Exists("templates/partials/header.dax")) File.WriteAllText("templates/partials/header.dax", Sample.Header);
        if (!File.Exists("templates/partials/footer.dax")) File.WriteAllText("templates/partials/footer.dax", Sample.Footer);
        if (!File.Exists("templates/partials/seo.dax")) File.WriteAllText("templates/partials/seo.dax", Sample.Seo);
        if (!File.Exists("public/css/style.css")) File.WriteAllText("public/css/style.css", Sample.Css);
        Console.WriteLine("[DAX] ready - yaml support + tags list");
    }
}

class DaxBuilder
{
    Dictionary<string, object> GlobalData = new();
    List<DaxPage> AllPages = new();
    Dictionary<string, List<DaxPage>> Collections = new();
    Dictionary<string, List<DaxPage>> TagsMap = new();
    List<string> AllUrls = new();
    DaxEngine Engine = new();

    public void Build()
    {
        try
        {
            Console.WriteLine($"");
            Console.WriteLine($"D.A.X - Dotnet by AXcora");
            Console.WriteLine($"");
            Console.WriteLine("[DAX] build start...");
            if (!Directory.Exists("site")) Directory.CreateDirectory("site");
            LoadData();
            LoadContent();
            BuildCollections();
            BuildTags();
            CleanSite();
            RenderPages();
            RenderTagPages();
            RenderTagsListPage();
            RenderSeo();
            CopyPublic();
            Console.WriteLine($"[DAX] built {AllUrls.Count} pages to /site - collections: {string.Join(",", Collections.Keys)} - tags: {TagsMap.Count}");
            Console.WriteLine($"[DAX] built at {DateTime.Now}");
            Console.WriteLine($"");
            Console.WriteLine($"https://dax.axcora.com");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[BUILD ERROR] " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }

    void LoadData()
    {
        GlobalData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists("_data")) return;
        foreach (var file in Directory.GetFiles("_data", "*.*"))
        {
            var ext = Path.GetExtension(file).ToLower();
            var name = Path.GetFileNameWithoutExtension(file);
            try
            {
                if (ext == ".json")
                {
                    var json = File.ReadAllText(file);
                    var doc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                    if (doc == null) continue;
                    var dict = JsonToDict(doc);
                    GlobalData[name] = dict;
                    if (name == "metadata" && dict.TryGetValue("site", out var so) && so is Dictionary<string, object> sd)
                    {
                        GlobalData["metadata"] = sd;
                        GlobalData["site"] = sd;
                    }
                }
                else if (ext == ".yaml" || ext == ".yml")
                {
                    var yaml = File.ReadAllText(file);
                    var dict = SimpleYaml.Parse(yaml);
                    GlobalData[name] = dict;
                    Console.WriteLine($"[DAX] loaded yaml: {name} with {dict.Count} keys");
                }
            }
            catch (Exception ex) { Console.WriteLine($"[DATA ERROR] {file}: {ex.Message}"); }
        }
    }

    static Dictionary<string, object> JsonToDict(Dictionary<string, JsonElement> src)
    {
        var d = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in src) d[kv.Key] = JsonElemToObj(kv.Value);
        return d;
    }
    static object JsonElemToObj(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.Object)
        {
            var dd = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in el.EnumerateObject()) dd[p.Name] = JsonElemToObj(p.Value);
            return dd;
        }
        if (el.ValueKind == JsonValueKind.Array)
        {
            var list = new List<object>();
            foreach (var e in el.EnumerateArray()) list.Add(JsonElemToObj(e));
            return list;
        }
        if (el.ValueKind == JsonValueKind.String) return el.GetString();
        if (el.ValueKind == JsonValueKind.Number) return el.TryGetInt64(out var l)? (object)l : el.GetDouble();
        if (el.ValueKind == JsonValueKind.True) return true;
        if (el.ValueKind == JsonValueKind.False) return false;
        return "";
    }

    void LoadContent()
    {
        AllPages = new();
        var mdFiles = Directory.GetFiles("content", "*.md", SearchOption.AllDirectories);
        foreach (var f in mdFiles)
        {
            var raw = File.ReadAllText(f);
            var (fm, body) = ParseFrontmatter(raw);
            var rel = Path.GetRelativePath("content", f).Replace("\\", "/");
            var isIndex = Path.GetFileNameWithoutExtension(f) == "index";
            string url;
            if (rel == "index.md") url = "/";
            else if (isIndex) url = "/" + Path.GetDirectoryName(rel).Replace("\\", "/") + "/";
            else
            {
                var noExt = rel.EndsWith(".md")? rel.Substring(0, rel.Length - 3) : rel;
                url = "/" + noExt.TrimEnd('/') + "/";
            }

            var dir = Path.GetDirectoryName(rel);
            string collectionName = null;
            if (!string.IsNullOrEmpty(dir) && dir!= "." &&!fm.ContainsKey("collection"))
            {
                collectionName = dir.Split('/')[0];
            }

            var page = new DaxPage
            {
                SourcePath = f,
                RelPath = rel,
                Url = url.Replace("//", "/"),
                Frontmatter = fm,
                MarkdownBody = body,
                HtmlBody = SimpleMarkdown.ToHtml(body),
                CollectionName = collectionName,
                IsController = fm.ContainsKey("collection") || rel == "tags.md"
            };
            AllPages.Add(page);
        }
    }

    void BuildCollections()
{
    Collections.Clear();
    if (Directory.Exists("content"))
    {
        foreach (var dir in Directory.GetDirectories("content"))
        {
            var name = Path.GetFileName(dir);
            if (!Collections.ContainsKey(name)) Collections[name] = new List<DaxPage>();
        }
    }
    foreach (var p in AllPages.Where(x =>!x.IsController && x.CollectionName!= null))
    {
        if (!Collections.ContainsKey(p.CollectionName)) Collections[p.CollectionName] = new List<DaxPage>();
        Collections[p.CollectionName].Add(p);
    }
    foreach (var k in Collections.Keys.ToList())
    {
        // urutkan terbaru dulu
        Collections[k] = Collections[k].OrderByDescending(x => x.Frontmatter.TryGetValue("date", out var d)? d?.ToString() : "").ThenBy(x => x.Url).ToList();

        for (int i = 0; i < Collections[k].Count; i++)
        {
            // i=0 paling baru, jadi Next = lebih baru, Prev = lebih lama (kaya blog biasa)
            Collections[k][i].PrevPost = i + 1 < Collections[k].Count? Collections[k][i + 1] : null;
            Collections[k][i].NextPost = i - 1 >= 0? Collections[k][i - 1] : null;
        }
    }
}

    void BuildTags()
    {
        TagsMap.Clear();
        foreach (var p in AllPages.Where(x =>!x.IsController))
        {
            if (!p.Frontmatter.TryGetValue("tags", out var t)) continue;
            var tags = ParseTags(t);
            foreach (var tag in tags)
            {
                if (!TagsMap.ContainsKey(tag)) TagsMap[tag] = new List<DaxPage>();
                TagsMap[tag].Add(p);
            }
        }
    }

    List<string> ParseTags(object raw)
    {
        var list = new List<string>();
        if (raw is List<object> lo) foreach (var o in lo) list.Add(o.ToString().Trim());
        else if (raw is string s)
        {
            s = s.Trim().Trim('[', ']').Trim();
            foreach (var part in s.Split(',')) if (!string.IsNullOrWhiteSpace(part)) list.Add(part.Trim().Trim('"', '\''));
        }
        return list.Where(x =>!string.IsNullOrWhiteSpace(x)).Distinct().ToList();
    }

    void CleanSite()
    {
        if (!Directory.Exists("site")) return;
        foreach (var f in Directory.GetFiles("site", "*", SearchOption.AllDirectories))
        {
            try { File.Delete(f); } catch { }
        }
    }

    void RenderPages()
    {
        foreach (var page in AllPages)
        {
            if (page.RelPath == "tags.md") continue;

            if (page.IsController)
            {
                var collName = page.Frontmatter["collection"].ToString();
                int perPage = 6;
                if (page.Frontmatter.ContainsKey("pagination") && int.TryParse(page.Frontmatter["pagination"]?.ToString(), out var pp)) perPage = pp;

                if (!Collections.TryGetValue(collName, out var items)) continue;
                var totalPages = (int)Math.Ceiling(items.Count / (double)perPage);
                if (totalPages == 0) totalPages = 1;

                for (int pg = 1; pg <= totalPages; pg++)
                {
                    var chunk = items.Skip((pg - 1) * perPage).Take(perPage).ToList();

                    // FIX PAGINASI - JANGAN KASIH STRING KOSONG, KASIH NULL
                    var pagination = new Dictionary<string, object>
                    {
                        ["items"] = chunk.Select(x => x.ToTemplateDict()).ToList(),
                        ["current_page"] = pg,
                        ["total_pages"] = totalPages,
                        ["total_items"] = items.Count,
                        ["has_prev"] = pg > 1,
                        ["has_next"] = pg < totalPages,
                        ["prev_url"] = pg > 1? (pg == 2? $"/{collName}/" : $"/{collName}/page/{pg - 1}/") : null,
                        ["next_url"] = pg < totalPages? $"/{collName}/page/{pg + 1}/" : null
                    };

                    var ctx = BuildContext(page, extra: new Dictionary<string, object> { ["pagination"] = pagination, ["collections"] = CollectionsToTemplate(), ["tags"] = TagsMap.Keys.ToList() });
                    ctx["content"] = page.HtmlBody;
                    var html = Engine.RenderWithLayouts(page.Frontmatter, ctx);
                    string outPath = pg == 1? $"site/{collName}/index.html" : $"site/{collName}/page/{pg}/index.html";
                    Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                    File.WriteAllText(outPath, html);
                    AllUrls.Add(pg == 1? $"/{collName}/" : $"/{collName}/page/{pg}/");
                }
            }
            else
{
    var ctx = BuildContext(page, extra: new Dictionary<string, object> {
        ["collections"] = CollectionsToTemplate(),
        ["tags"] = TagsMap.Keys.ToList(),
        ["all_tags"] = TagsMap.Keys.ToList(),
        ["prev_post"] = page.PrevPost?.ToTemplateDict(),
        ["next_post"] = page.NextPost?.ToTemplateDict(),
        ["has_prev"] = page.PrevPost!= null,
        ["has_next"] = page.NextPost!= null
    });
    ctx["content"] = page.HtmlBody;
    var html = Engine.RenderWithLayouts(page.Frontmatter, ctx);
    string outPath = page.Url == "/"? "site/index.html" : $"site{page.Url}index.html";
    Directory.CreateDirectory(Path.GetDirectoryName(outPath));
    File.WriteAllText(outPath, html);
    AllUrls.Add(page.Url);
}
        }
    }

    Dictionary<string, object> CollectionsToTemplate()
    {
        var d = new Dictionary<string, object>();
        foreach (var kv in Collections) d[kv.Key] = kv.Value.Select(x => x.ToTemplateDict()).ToList();
        return d;
    }

    void RenderTagPages()
    {
        if (!File.Exists("templates/layouts/tag.dax")) return;
        foreach (var kv in TagsMap)
        {
            var tag = kv.Key;
            var tagUrl = $"/tags/{Slug(tag)}/";
            var ctx = new Dictionary<string, object>(GlobalData, StringComparer.OrdinalIgnoreCase);
            ctx["tag"] = tag;
            ctx["url"] = tagUrl;
            ctx["image"] = "/img/og.jpg";
            ctx["title"] = $"Tag: {tag}";
            ctx["description"] = $"Posts tagged {tag}";
            ctx["posts"] = kv.Value.Select(x => x.ToTemplateDict()).ToList();
            ctx["collections"] = CollectionsToTemplate();
            ctx["tags"] = TagsMap.Keys.ToList();
            ctx["all_tags"] = TagsMap.Keys.ToList();
            ctx["content"] = "";
            var fm = new Dictionary<string, object> { ["layout"] = "tag.dax", ["title"] = $"Tag: {tag}" };
            var html = Engine.RenderWithLayouts(fm, ctx);
            var outPath = $"site/tags/{Slug(tag)}/index.html";
            Directory.CreateDirectory(Path.GetDirectoryName(outPath));
            File.WriteAllText(outPath, html);
            AllUrls.Add(tagUrl);
        }
    }

    void RenderTagsListPage()
    {
        var ctx = new Dictionary<string, object>(GlobalData, StringComparer.OrdinalIgnoreCase);
        var tagList = TagsMap.Select(kv => new Dictionary<string, object> {
            ["name"] = kv.Key,
            ["slug"] = Slug(kv.Key),
            ["count"] = kv.Value.Count,
            ["url"] = $"/tags/{Slug(kv.Key)}/"
        }).Cast<object>().ToList();

        ctx["tags"] = TagsMap.Keys.ToList();
        ctx["all_tags"] = tagList;
        ctx["collections"] = CollectionsToTemplate();
        ctx["title"] = "All Tags";
        ctx["content"] = "";

        var tagsPage = AllPages.FirstOrDefault(x => x.RelPath == "tags.md");
        Dictionary<string, object> fm;
        if (tagsPage!= null)
        {
            fm = tagsPage.Frontmatter;
            if (!fm.ContainsKey("layout")) fm["layout"] = "tags-list.dax";
            ctx["content"] = tagsPage.HtmlBody;
            foreach (var kv in tagsPage.Frontmatter) if (!ctx.ContainsKey(kv.Key)) ctx[kv.Key] = kv.Value;
        }
        else
        {
            fm = new Dictionary<string, object> { ["layout"] = "tags-list.dax", ["title"] = "All Tags" };
        }

        var html = Engine.RenderWithLayouts(fm, ctx);
        var outPath = "site/tags/index.html";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, html);
        AllUrls.Add("/tags/");
    }

    static string Slug(string s) => Regex.Replace(s.ToLower().Trim(), @"[^a-z0-9]+", "-").Trim('-');

    Dictionary<string, object> BuildContext(DaxPage page, Dictionary<string, object> extra = null)
    {
        var ctx = new Dictionary<string, object>(GlobalData, StringComparer.OrdinalIgnoreCase);
        foreach (var kv in page.Frontmatter) ctx[kv.Key] = kv.Value;
        ctx["url"] = page.Url;
        ctx["page"] = page.ToTemplateDict();
        ctx["metadata"] = GlobalData.ContainsKey("metadata")? GlobalData["metadata"] : new Dictionary<string, object>();
        ctx["config"] = GlobalData.ContainsKey("config")? GlobalData["config"] : new Dictionary<string, object>();
        ctx["site"] = ctx["metadata"];
        if (ctx["metadata"] is Dictionary<string, object> md)
        {
            if (!md.ContainsKey("url")) md["url"] = "http://localhost:8080";
        }
        if (extra!= null) foreach (var kv in extra) ctx[kv.Key] = kv.Value;
        return ctx;
    }

    void RenderSeo()
    {
        var siteUrl = "http://localhost:8080";
        if (GlobalData.TryGetValue("metadata", out var mo) && mo is Dictionary<string, object> md && md.TryGetValue("url", out var u)) siteUrl = u.ToString();
        siteUrl = siteUrl.TrimEnd('/');
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        foreach (var url in AllUrls.Distinct()) sb.AppendLine($"<url><loc>{siteUrl}{url}</loc></url>");
        sb.AppendLine("</urlset>");
        File.WriteAllText("site/sitemap.xml", sb.ToString());
        File.WriteAllText("site/robots.txt", $"User-agent: *\nAllow: /\nSitemap: {siteUrl}/sitemap.xml\n");
        var posts = Collections.Values.SelectMany(x => x).OrderByDescending(x => x.Frontmatter.TryGetValue("date", out var d)? d?.ToString() : "").Take(20).ToList();
        var rss = new StringBuilder();
        rss.AppendLine($"<?xml version=\"1.0\"?><rss version=\"2.0\"><channel><title>DAX Feed</title><link>{siteUrl}</link><description>DAX</description>");
        foreach (var p in posts) rss.AppendLine($"<item><title><![CDATA[{p.Frontmatter.GetValueOrDefault("title", "")}]]></title><link>{siteUrl}{p.Url}</link><guid>{siteUrl}{p.Url}</guid></item>");
        rss.AppendLine("</channel></rss>");
        File.WriteAllText("site/rss.xml", rss.ToString());
    }

    void CopyPublic()
    {
        if (!Directory.Exists("public")) return;
        foreach (var f in Directory.GetFiles("public", "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath("public", f);
            var dest = Path.Combine("site", rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            File.Copy(f, dest, true);
        }
    }

    static (Dictionary<string, object>, string) ParseFrontmatter(string raw)
    {
        var m = Regex.Match(raw, @"^---\s*\r?\n(.*?)\r?\n---\s*\r?\n(.*)", RegexOptions.Singleline);
        if (!m.Success) return (new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase), raw);
        var yaml = m.Groups[1].Value;
        var body = m.Groups[2].Value;
        var dict = SimpleYaml.Parse(yaml);
        return (dict, body);
    }
}

class DaxPage
{
    public string SourcePath = ""; public string RelPath = ""; public string Url = "";
    public Dictionary<string, object> Frontmatter = new(StringComparer.OrdinalIgnoreCase);
    public string MarkdownBody = ""; public string HtmlBody = "";
    public string CollectionName; public bool IsController;
    public DaxPage PrevPost; public DaxPage NextPost;
    public Dictionary<string, object> ToTemplateDict()
    {
        var d = new Dictionary<string, object>(Frontmatter, StringComparer.OrdinalIgnoreCase);
        d["url"] = Url;
        d["content"] = HtmlBody;
        return d;
    }
}

class DaxEngine
{
    string LoadLayout(string name)
    {
        var clean = name.Replace(".dax","").Trim('/','\\');
        var paths = new[] {
            $"templates/layouts/{clean}.dax",
            $"templates/layouts/{clean}",
            $"templates/{clean}.dax"
        };
        foreach (var p in paths) if (File.Exists(p)) return File.ReadAllText(p);
        return null;
    }

    string LoadPartial(string name)
    {
        var clean = name.Replace(".dax","").Trim('/','\\','\"','\'');
        var paths = new[] {
            $"templates/partials/{clean}.dax",
            $"templates/partials/{clean}",
            $"templates/layouts/{clean}.dax" // fallback kalau partial gak ada
        };
        foreach (var p in paths) if (File.Exists(p)) return File.ReadAllText(p);
        return $"<!-- partial not found: {name} -->";
    }

    public string RenderWithLayouts(Dictionary<string, object> fm, Dictionary<string, object> ctx)
    {
        string currentContent = ctx.ContainsKey("content")? ctx["content"]?.ToString() : "";
        var layoutChain = new List<string>();
        if (fm.TryGetValue("layout", out var l)) layoutChain.Add(l.ToString());
        string rendered = currentContent;
        int level = 0;
        while (layoutChain.Count > 0 && level < 5)
        {
            var layoutName = layoutChain[0]; layoutChain.RemoveAt(0);
            var tpl = LoadLayout(layoutName);
            if (tpl == null) break;
            var (innerFm, innerBody) = ParseLayoutFrontmatter(tpl);
            if (innerFm.TryGetValue("layout", out var parent)) layoutChain.Add(parent.ToString());
            var useBody = string.IsNullOrWhiteSpace(innerBody)? tpl : innerBody;
            var newCtx = new Dictionary<string, object>(ctx, StringComparer.OrdinalIgnoreCase);
            newCtx["content"] = rendered;
            rendered = RenderString(useBody, newCtx);
            level++;
        }
        if (level == 0) rendered = RenderString(currentContent, ctx);
        return rendered;
    }

    (Dictionary<string, object>, string) ParseLayoutFrontmatter(string raw)
    {
        var m = Regex.Match(raw, @"^---\s*\r?\n(.*?)\r?\n---\s*\r?\n(.*)", RegexOptions.Singleline);
        if (!m.Success) return (new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase), raw);
        var dict = SimpleYaml.Parse(m.Groups[1].Value);
        return (dict, m.Groups[2].Value);
    }

    public string RenderString(string tpl, Dictionary<string, object> ctx)
    {
        if (string.IsNullOrEmpty(tpl)) return "";

        tpl = Regex.Replace(tpl, @"\{%\s*include\s+([^\s%]+)\s*%}", m =>
        {
            var incName = m.Groups[1].Value.Trim('\"', '\'');
            var incTpl = LoadPartial(incName);
            return RenderString(incTpl, ctx);
        });

        tpl = RenderFors(tpl, ctx);
        tpl = RenderIfs(tpl, ctx);
        tpl = Regex.Replace(tpl, @"\{\{\s*([^\}]+?)\s*\}\}", m =>
        {
            var raw = m.Groups[1].Value.Trim();
            if (raw.Contains(" or "))
            {
                foreach (var p in Regex.Split(raw, @"\s+or\s+"))
                {
                    var v = Resolve(p.Trim(), ctx);
                    if (v!= null &&!string.IsNullOrWhiteSpace(v.ToString())) return v.ToString();
                }
                return "";
            }
            var val = Resolve(raw, ctx);
            return val?.ToString()?? "";
        });
        return tpl;
    }

    string RenderFors(string tpl, Dictionary<string, object> ctx)
    {
        var pattern = @"\{%\s*for\s+(\w+)\s+(?:in\s+)?([^\s%]+)(?:\s+limit\s*:\s*(\d+))?\s*%\}(.*?)\{%\s*endfor\s*%\}";
        while (true)
        {
            var m = Regex.Match(tpl, pattern, RegexOptions.Singleline);
            if (!m.Success) break;
            var varName = m.Groups[1].Value;
            var colExpr = m.Groups[2].Value.Trim();
            var limitStr = m.Groups[3].Value;
            var inner = m.Groups[4].Value;
            var rawCol = Resolve(colExpr, ctx);
            List<object> list = new();
            if (rawCol is List<object> lo) list = lo;
            else if (rawCol is IEnumerable<object> en) list = en.ToList();
            if (!string.IsNullOrEmpty(limitStr) && int.TryParse(limitStr, out var lim)) list = list.Take(lim).ToList();
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                var childCtx = new Dictionary<string, object>(ctx, StringComparer.OrdinalIgnoreCase);
                childCtx[varName] = item;
                sb.Append(RenderString(inner, childCtx));
            }
            tpl = tpl.Substring(0, m.Index) + sb.ToString() + tpl.Substring(m.Index + m.Length);
        }
        return tpl;
    }

        string RenderIfs(string tpl, Dictionary<string, object> ctx)
    {
        
        var pattern = @"\{%\s*if\s+([^%]+?)\s*%\}((?:(?!\{%\s*if).)*?)(?:\{%\s*else\s*%\}((?:(?!\{%\s*if).)*?))?\{%\s*endif\s*%\}";

        while (true)
        {
            var m = Regex.Match(tpl, pattern, RegexOptions.Singleline);
            if (!m.Success) break;

            var cond = m.Groups[1].Value.Trim();
            var ifBlock = m.Groups[2].Value;
            var elseBlock = m.Groups[3].Success? m.Groups[3].Value : "";
            var ok = EvalCondition(cond, ctx);
            var chosen = ok? ifBlock : elseBlock;

            var renderedChosen = RenderString(chosen, ctx);

            tpl = tpl.Substring(0, m.Index) + renderedChosen + tpl.Substring(m.Index + m.Length);
        }
        return tpl;
    }

    bool EvalCondition(string cond, Dictionary<string, object> ctx)
    {
        cond = cond.Trim();
        if (cond.Contains(" OR ") || cond.Contains(" or ") || cond.Contains(" || "))
        {
            var parts = Regex.Split(cond, @"\s+(?:OR|or|\|\|)\s+");
            return parts.Any(p => EvalCondition(p, ctx));
        }
        if (cond.Contains(" AND ") || cond.Contains(" and ") || cond.Contains(" && "))
        {
            var parts = Regex.Split(cond, @"\s+(?:AND|and|&&)\s+");
            return parts.All(p => EvalCondition(p, ctx));
        }
        if (cond.Contains("==")) { var sp = cond.Split(new[] { "==" }, StringSplitOptions.None); return Resolve(sp[0].Trim(), ctx)?.ToString() == sp[1].Trim().Trim('\"', '\''); }
        if (cond.Contains("!=")) { var sp = cond.Split(new[] { "!=" }, StringSplitOptions.None); return Resolve(sp[0].Trim(), ctx)?.ToString()!= sp[1].Trim().Trim('\"', '\''); }
        var v = Resolve(cond, ctx);
        if (v == null) return false;
        if (v is bool b) return b;
        if (v is string s) return!string.IsNullOrWhiteSpace(s);
        if (v is List<object> l) return l.Count > 0;
        return true;
    }

    object Resolve(string path, Dictionary<string, object> ctx)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";
        path = path.Trim();
        if (path.StartsWith("\"") && path.EndsWith("\"")) return path.Trim('"');
        var parts = path.Split('.');
        object cur = null;
        if (ctx.TryGetValue(parts[0], out var first)) cur = first;
        else return null;
        for (int i = 1; i < parts.Length; i++)
        {
            if (cur is Dictionary<string, object> d)
            {
                if (!d.TryGetValue(parts[i], out cur)) return null;
            }
            else return null;
        }
        return cur;
    }
}

static class SimpleYaml
{
    public static Dictionary<string, object> Parse(string yaml)
    {
        var root = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var lines = yaml.Split('\n');
        var stack = new Stack<(int indent, object container)>();
        stack.Push((-1, root));

        for (int i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            if (string.IsNullOrWhiteSpace(raw) || raw.TrimStart().StartsWith("#")) continue;
            int indent = 0;
            while (indent < raw.Length && (raw[indent] == ' ' || raw[indent] == '\t')) indent++;
            var trimmed = raw.Trim();
            if (trimmed.Length == 0) continue;

            while (stack.Count > 1 && stack.Peek().indent >= indent) stack.Pop();

            if (trimmed.StartsWith("- "))
            {
                var afterDash = trimmed.Substring(2).Trim();
                List<object> targetList = null;
                foreach (var st in stack)
                {
                    if (st.container is List<object> ll) { targetList = ll; break; }
                }
                if (targetList == null) continue;

                if (afterDash.Contains(":"))
                {
                    var colon = afterDash.IndexOf(':');
                    var k = afterDash.Substring(0, colon).Trim();
                    var v = afterDash.Substring(colon + 1).Trim().Trim('"', '\'');
                    var obj = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    if (!string.IsNullOrEmpty(v)) obj[k] = v;
                    targetList.Add(obj);
                    stack.Push((indent, obj));
                }
                else
                {
                    targetList.Add(afterDash.Trim('"', '\''));
                }
            }
            else if (trimmed.Contains(":"))
            {
                var colon = trimmed.IndexOf(':');
                var key = trimmed.Substring(0, colon).Trim();
                var valRaw = trimmed.Substring(colon + 1).Trim();

                Dictionary<string, object> parentDict = null;
                foreach (var st in stack)
                {
                    if (st.container is Dictionary<string, object> d) { parentDict = d; break; }
                }
                if (parentDict == null) continue;

                if (string.IsNullOrEmpty(valRaw))
                {
                    bool nextIsList = false;
                    int j = i + 1;
                    while (j < lines.Length)
                    {
                        var nxt = lines[j];
                        if (string.IsNullOrWhiteSpace(nxt)) { j++; continue; }
                        var nt = nxt.Trim();
                        if (nt.StartsWith("- ")) { nextIsList = true; break; }
                        break;
                    }
                    if (nextIsList)
                    {
                        var newList = new List<object>();
                        parentDict[key] = newList;
                        stack.Push((indent, newList));
                    }
                    else
                    {
                        var newDict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        parentDict[key] = newDict;
                        stack.Push((indent, newDict));
                    }
                }
                else
                {
                    if (valRaw.StartsWith("[") && valRaw.EndsWith("]"))
                    {
                        var inner = valRaw.Trim('[', ']').Trim();
                        var arr = new List<object>();
                        if (!string.IsNullOrEmpty(inner))
                        {
                            foreach (var part in inner.Split(',')) arr.Add(part.Trim().Trim('"', '\''));
                        }
                        parentDict[key] = arr;
                    }
                    else
                    {
                        parentDict[key] = valRaw.Trim('"', '\'');
                    }
                }
            }
        }
        return root;
    }
}

static class SimpleMarkdown
{
    public static string ToHtml(string md)
    {
        if (string.IsNullOrWhiteSpace(md)) return "";
        var html = md;

        // Code block ```lang\n...```
        html = Regex.Replace(html, @"```(\w*)\r?\n(.*?)```", m => "<pre><code>" + System.Net.WebUtility.HtmlEncode(m.Groups[2].Value) + "</code></pre>", RegexOptions.Singleline);

        // Inline code
        html = Regex.Replace(html, @"`([^`]+)`", "<code>$1</code>");

        // TABLE SUPPORT - | a | b | \n | --- | --- | \n | c | d |
        html = Regex.Replace(html, @"((?:^\|.*\|\s*$\n?)+)", m => {
            var tableBlock = m.Value.Trim();
            var lines = tableBlock.Split('\n').Where(l =>!string.IsNullOrWhiteSpace(l)).ToArray();
            if (lines.Length < 2) return m.Value;

            var sb = new StringBuilder();
            sb.AppendLine("<div class=\"table-responsive\"><table>");

            // Header
            var headers = lines[0].Split('|').Where(c =>!string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToArray();
            sb.AppendLine("<thead><tr>");
            foreach (var h in headers) sb.AppendLine($"<th>{h}</th>");
            sb.AppendLine("</tr></thead>");

            // Body (skip separator line index 1)
            sb.AppendLine("<tbody>");
            for (int i = 2; i < lines.Length; i++)
            {
                var cells = lines[i].Split('|').Where(c =>!string.IsNullOrWhiteSpace(c) || c == "").Select(c => c.Trim()).ToArray();
                // filter empty from leading/trailing |
                cells = lines[i].Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray();
                if (cells.Length == 0) continue;
                sb.AppendLine("<tr>");
                foreach (var cell in cells) sb.AppendLine($"<td>{ParseInline(cell)}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody></table></div>");
            return sb.ToString();
        }, RegexOptions.Multiline);

        // Blockquote
        html = Regex.Replace(html, @"^>\s*(.+)$", "<blockquote>$1</blockquote>", RegexOptions.Multiline);

        // HR
        html = Regex.Replace(html, @"^---\s*$", "<hr/>", RegexOptions.Multiline);

        // Headings
        html = Regex.Replace(html, @"^### (.+)$", "<h3>$1</h3>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"^## (.+)$", "<h2>$1</h2>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"^# (.+)$", "<h1>$1</h1>", RegexOptions.Multiline);

        // Bold + Italic
        html = Regex.Replace(html, @"\*\*\*(.+?)\*\*\*", "<strong><em>$1</em></strong>");
        html = Regex.Replace(html, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        html = Regex.Replace(html, @"\*(.+?)\*", "<em>$1</em>");

        // Images & Links (must before list)
        html = Regex.Replace(html, @"!\[([^\]]*)\]\(([^)]+)\)", "<img class=\"img-fluid\" src=\"$2\" alt=\"$1\" />");
        html = Regex.Replace(html, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");

        // Checklist - [x] and [ ]
        html = Regex.Replace(html, @"^\- \[x\] (.+)$", "<li class=\"check done\">✅ $1</li>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"^\- \[ \] (.+)$", "<li class=\"check\">⬜ $1</li>", RegexOptions.Multiline);

        // Unordered list
        html = Regex.Replace(html, @"^\- (.+)$", "<li>$1</li>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"(<li>.*</li>\s*)+", m => "<ul>" + m.Value + "</ul>", RegexOptions.Singleline);

        // Paragraphs - wrap leftover lines
        html = Regex.Replace(html, @"^(?!<[h|u|o|l|p|b|t|d|i|h|r])(.+)$", "<p>$1</p>", RegexOptions.Multiline);

        return html;
    }

    static string ParseInline(string text)
    {
        text = Regex.Replace(text, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        text = Regex.Replace(text, @"`([^`]+)`", "<code>$1</code>");
        text = Regex.Replace(text, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\">$1</a>");
        return text;
    }
}
class DaxServer
{
    public void Start()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("[DAX] serving at http://localhost:8080/");

        var watcher = new FileSystemWatcher(".") { IncludeSubdirectories = true, EnableRaisingEvents = true };
        DateTime lastBuild = DateTime.Now;
        watcher.Changed += (s, e) =>
        {
            try
            {
                var full = e.FullPath.ToLower();
                if (full.Contains("\\site\\") || full.Contains("/site/") || full.Contains("\\bin\\") || full.Contains("\\obj\\") || full.Contains("\\.git\\")) return;
                if ((DateTime.Now - lastBuild).TotalMilliseconds < 500) return;
                lastBuild = DateTime.Now;
                Console.WriteLine($"[DAX] changed {e.Name} -> rebuild");
                new DaxBuilder().Build();
            }
            catch { }
        };

        while (true)
        {
            var ctx = listener.GetContext();
            var rawPath = ctx.Request.Url.AbsolutePath;
            var path = Uri.UnescapeDataString(rawPath.TrimStart('/'));
            if (string.IsNullOrEmpty(path)) path = "index.html";
            var filePath = Path.Combine("site", path.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(filePath)) filePath = Path.Combine(filePath, "index.html");
            else if (!File.Exists(filePath) &&!Path.HasExtension(filePath))
            {
                var idx = Path.Combine(filePath, "index.html");
                if (File.Exists(idx)) filePath = idx;
            }
            if (File.Exists(filePath))
            {
                var bytes = File.ReadAllBytes(filePath);
                ctx.Response.ContentType = GetMimeType(filePath);
                ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            }
            else
            {
                ctx.Response.StatusCode = 404;
                var b = Encoding.UTF8.GetBytes($"404 not found: {rawPath}");
                ctx.Response.ContentType = "text/plain";
                ctx.Response.OutputStream.Write(b, 0, b.Length);
            }
            ctx.Response.Close();
        }
    }

    static string GetMimeType(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".ico" => "image/x-icon",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            ".ttf" => "font/ttf",
            ".html" or ".htm" => "text/html",
            _ => "application/octet-stream"
        };
    }
}

static class Sample
{
    public static string Metadata = "{\"site\":{\"title\":\"DAX\",\"description\":\"Pure C# SSG\",\"url\":\"http://localhost:8080\"}}";
    public static string ConfigYaml = "site_name: \"DAX\"\npagination_default: \"6\"\nlist:\n - title: \"Blog\"\n url: \"/blog/\"\n description: \"Latest news\"\n";
    public static string IndexMd = "---\ntitle: Home\nlayout: home.dax\ndescription: Welcome\n---\nWelcome to DAX!";
    public static string TagsMd = "---\ntitle: All Tags\nlayout: tags-list.dax\n---\n";
    public static string PostsMd = "---\nlayout: posts-list.dax\ntitle: Blog\ncollection: posts\npagination: 6\n---\n";
    public static string Post1 = "---\ntitle: Hello DAX\nlayout: post.dax\ndate: 2026-08-01\ntags: [csharp, dax]\n---\nContent hello";
    public static string BaseDax = "<!doctype html><html><head>{% include seo.dax %}<link rel=\"stylesheet\" href=\"/css/style.css\"></head><body>{% include header.dax %}<main>{{ content }}</main>{% include footer.dax %}</body></html>";
    public static string Header = "<header><a href=\"/\">DAX</a> | <a href=\"/posts/\">Posts</a> | <a href=\"/tags/\">Tags</a></header>";
    public static string Footer = "<footer>DAX</footer>";
    public static string Seo = "<meta name=\"generator\" content=\"DAX\"><title>{{ title }}</title>";
    public static string HomeDax = "<h1>{{ title }}</h1><div>{{ content }}</div>";
    public static string ListDax = "---\nlayout: base.dax\n---\n<h1>{{ title }}</h1>{% for post in pagination.items %}<div><a href=\"{{ post.url }}\">{{ post.title }}</a></div>{% endfor %}<div>{% if pagination.has_prev %}<a href=\"{{ pagination.prev_url }}\">Prev</a>{% endif %} Page {{ pagination.current_page }}/{{ pagination.total_pages }} {% if pagination.has_next %}<a href=\"{{ pagination.next_url }}\">Next</a>{% endif %}</div>";
    public static string PostDax = "---\nlayout: base.dax\n---\n<h1>{{ title }}</h1><div>{{ content }}</div>";
    public static string TagDax = "---\nlayout: base.dax\n---\n<h1>Tag: {{ tag }}</h1>{% for post in posts %}<div><a href=\"{{ post.url }}\">{{ post.title }}</a></div>{% endfor %}";
    public static string TagsListDax = "---\nlayout: base.dax\n---\n<h1>{{ title }}</h1>{% for t in all_tags %}<div><a href=\"{{ t.url }}\">{{ t.name }}</a> ({{ t.count }})</div>{% endfor %}";
    public static string Css = "body{font-family:system-ui;max-width:800px;margin:auto;padding:20px}";
}