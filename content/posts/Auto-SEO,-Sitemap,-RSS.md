---
title: Auto SEO, Sitemap, RSS and OpenGraph in DAX C#
description: How DAX auto generates sitemap.xml, rss.xml, and SEO meta without plugins
image: /img/favicon.webp
layout: post.dax
date: 2026-08-02
tags: [seo, sitemap, rss, performance]
---

In Jekyll you need `jekyll-sitemap` plugin. In DAX it's built-in.

After `dotnet run -- build`, you get:

- `/site/sitemap.xml` - all collections
- `/site/rss.xml` - blog posts sorted by date
- `/site/robots.txt`
- Auto `<meta>` canonical, og:image from frontmatter `image:`

Just set in frontmatter:

```yaml
title: My Post
description: SEO description
image: /img/dax-primary-logo.webp
```

DAX injects:

```html
<meta name="description" content="...">
<meta property="og:image" content="...">
<link rel="canonical" href="...">
```

Lighthouse 100/100 out of the box. No plugin needed.
