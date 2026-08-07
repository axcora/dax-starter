---
title: Deploy DAX Static Site to GitHub Pages, Cloudflare, and Vercel
description: Step by step deploying DAX /site output to GitHub Pages with custom domain and Cloudflare
image: /img/icon.webp
layout: post.dax
date: 2026-08-06
tags: [deploy, github-pages, cloudflare, hosting]
---

DAX output is just `/site` folder - pure HTML/CSS.

### GitHub Pages (Actions)

```yaml
- uses: actions/upload-pages-artifact@v3
  with:
    path: ./site
- uses: actions/deploy-pages@v4
```

### GitHub Pages (gh-pages branch)

```yaml
- uses: JamesIves/github-pages-deploy-action@v4
  with:
    folder: site
    branch: gh-pages
```

### Cloudflare Pages

Connect repo, set build command: `dotnet run -- build`, output: `site`.

### Custom Domain

Add `public/CNAME` with `dax.axcora.com` and A records:

```
185.199.108.153
185.199.109.153
185.199.110.153
185.199.111.153
```

DAX auto copies `public/` to `site/`, so CNAME stays.
