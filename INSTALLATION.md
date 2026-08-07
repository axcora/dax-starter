# Installation & Getting Started - DAX

> Pure C# Slim SSG - No Node, No Blazor, Just CMD.

## Requirements

- .NET SDK 9.0+ (or 8.0 works) - https://dotnet.microsoft.com/download
- Windows CMD / Linux / Mac Terminal (no PowerShell needed)
- Git (for deploy)

Check:
```bat
dotnet --version
:: must show 9.x or 8.x
```

---

## 1. Create New Project

### Option A: Slim Version (Recommended)

```
git clone https://github.com/mesinkasir/dax.git
cd dax
dotnet run -- build
dotnet run -- start
open localhost:8080
```

### Option B: DAX Version (Recommended)

```
git clone https://github.com/mesinkasir/dax.git
cd dax
dotnet publish -c Release -o .
dax build
dax start
open localhost:8080
```

default structure:

```
_data/
  metadata.json
  config.yaml
content/
  index.md
  posts.md
  posts/hello.md
templates/
  layouts/
    base.dax
    home.dax
    posts-list.dax
    post.dax
    tag.dax
    tags-list.dax
  partials/
    header.dax
    footer.dax
    seo.dax
public/
  css/style.css
site/  (generated)
```

### Option B: Clone DAX Template

```bat
git clone https://github.com/username/dax-template my-blog
cd my-blog
dotnet run -- build
```

---

## 2. Project Structure Explained

```
dax/
├── Program.cs              # CORE - 1 file SSG engine (800 lines)
├── Dax.csproj              # Project file - net9.0, PublishSingleFile
├── vercel.json             # Vercel deploy config
├── netlify.toml            # Netlify deploy config
├── .gitignore
├── README.md
├── INSTALLATION.md         # This file
├── DEPLOY.md
├── _data/                  # GLOBAL DATA (auto-loaded as variables)
│   ├── metadata.json       # {{ metadata.title }}, {{ site.url }}
│   ├── config.yaml         # {{ config.site_name }}, {{ config.list }}
│   └── *.json / *.yaml     # Any file -> auto variable
├── content/                # CONTENT (file-based routing + auto collections)
│   ├── index.md            # -> /  (free frontmatter: hero, etc)
│   ├── about.md            # -> /about/
│   ├── tags.md             # -> /tags/ (auto tags list page)
│   ├── posts.md            # Controller: collection: posts, pagination: 6 -> /posts/
│   ├── posts/
│   │   ├── hello-world.md  # -> /posts/hello-world/ + tags: [csharp]
│   │   └── second.md
│   └── projects/           # Auto -> collections.projects -> /projects/
│       └── dax.md
├── templates/
│   ├── layouts/
│   │   ├── base.dax        # Base HTML + {{ content }}
│   │   ├── home.dax        # Home uses hero.list, config.list
│   │   ├── posts-list.dax  # Uses pagination.items
│   │   ├── post.dax        # Single + prev_post/next_post
│   │   ├── tag.dax         # Single tag: /tags/csharp/
│   │   └── tags-list.dax   # All tags: /tags/
│   └── partials/
│       ├── header.dax      # {% for n in config.list %}
│       ├── footer.dax
│       └── seo.dax
├── public/                 # Static assets copied to site/
│   ├── css/style.css       # -> site/css/style.css
│   ├── images/
│   └── favicon.ico
└── site/                   # GENERATED - deploy this folder
    ├── index.html
    ├── posts/
    │   ├── index.html
    │   └── page/2/index.html
    ├── tags/
    │   ├── index.html
    │   └── csharp/index.html
    ├── sitemap.xml
    ├── robots.txt
    ├── rss.xml
    └── css/style.css
```

---

## 3. Commands

### Build
```bat
dotnet run -- build
:: or after publish
bin\dax.exe build
```
Generates `site/` - 11 pages example.

### Dev Server (with watch)
```bat
dotnet run -- start
:: Open http://localhost:8080/
:: Edit any file in content/, templates/, _data/, public/ -> auto rebuild
```

### Init (create default files if missing)
```bat
dotnet run -- init
```

### Publish as single exe (like cax.exe)
```bat
dotnet publish -c Release -r win-x64 --self-contained -o bin
copy bin\DAX.exe bin\dax.exe
bin\dax.exe build
bin\dax.exe start

:: Linux
dotnet publish -c Release -r linux-x64 --self-contained -o bin
./bin/dax build
```

---

## 4. Writing Content

### Simple Page `content/about.md`
```md
---
title: About Us
layout: post.dax
description: About DAX
---
# About

Welcome to DAX!
```

-> `/about/`

### With Free Nested Frontmatter `content/index.md`
```md
---
title: Home
layout: home.dax
hero:
  title: Hello DAX
  description: Pure C# SSG
  list:
    - name: Blog
      url: /posts/
    - name: GitHub
      url: https://github.com/
image: /images/hero.png
---

Welcome content here
```

In `home.dax`:
```dax
<h1>{{ hero.title }}</h1>
{% for item in hero.list %}
  <a href="{{ item.url }}">{{ item.name }}</a>
{% endfor %}
{% if image %}<img src="{{ image }}"/>{% endif %}
```

**Any key you add in frontmatter is available** - unlimited nesting.

### Blog Post `content/posts/my-first.md`
```md
---
title: My First Post
date: 2026-08-03
tags: [csharp, dax, ssg]
description: First post
layout: post.dax
---
Content in **markdown**.
```

### Collection Controller `content/posts.md`
```md
---
title: Blog
layout: posts-list.dax
collection: posts
pagination: 6
---
All posts here
```
This generates `/posts/` + `/posts/page/2/` etc with `pagination.items`.

---

## 5. Data Files

`_data/config.yaml`:
```yaml
site_name: "DAX"
pagination_default: 6
list:
  - title: "Blog"
    url: "/blog/"
    description: "News"
  - title: "About"
    url: "/about/"
```

Use in any template:
```dax
{% for c in config.list %}
  <a href="{{ c.url }}">{{ c.title }}</a> - {{ c.description }}
{% endfor %}
```

Supports: `.json`, `.yaml`, `.yml` - nested unlimited.

---

## 6. Templating

- `{{ title }}`, `{{ hero.title }}`, `{{ config.list.0.title }}`
- `{% for post in collections.posts limit:3 %}`
- `{% for n in config.list %}` or `{% for n config.list %}` (both work)
- `{% if image %}...{% endif %}`
- `{% include header.dax %}`
- `{{ content }}` = rendered markdown + layout chain

---

## 7. Tags

Add `tags: [csharp, dax]` to any post.

- `/tags/` auto list all tags (`all_tags` with name, slug, count, url)
- `/tags/csharp/` auto pages

---

## 8. Deploy

See `DEPLOY.md` - supports GitHub Pages, Vercel, Netlify, Cloudflare.

Quickest:
```bat
git init
git add .
git commit -m "init dax"
git push to GitHub
:: Set GitHub Settings > Pages > Source: GitHub Actions
:: Workflow .github/workflows/deploy.yml auto deploys site/
```

---

## 9. Why DAX not Blazor?

DAX = static HTML 50ms load, 0KB JS.
Blazor = WASM 2s load, 500KB+ JS, needs hydration.

Use DAX for blogs/docs/portfolios. Use Blazor for apps.

---

## Troubleshooting

**Error: site/ 404?**
- Watch excludes `site/`, `bin/`, `obj/` now. If loop, update Program.cs to latest.

**Error: {{ config.list }} empty?**
- Ensure `_data/config.yaml` exists, not `.yml` typo, and `dotnet run -- build` shows `[DAX] loaded yaml: config`.

**Error: {% for n config.list %} outputs raw?**
- Use `{% for n in config.list %}` - latest Program.cs supports both but `in` is standard.

