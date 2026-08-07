---
title: DAX Roadmap v2: AOT, Image Optimization, and CMS Mode
description: What's next for DAX C# SSG - Native AOT binary, image webp optimization, and headless CMS support
image: /img/favicon.webp
layout: post.dax
date: 2026-08-07
tags: [roadmap, dax, dotnet8, future]
---

DAX v1 is stable: scan, render, copy.

### DAX v2 Plans

1. **Native AOT Binary** - `dax.exe` 15MB single file, no .NET SDK needed. `dax build` in 0.05s.

2. **Image Optimizer** - Auto webp/avif conversion for `/public/img/*` using ImageSharp.

3. **CMS Mode** - `dotnet run -- serve` with file watcher + WebSocket live reload. No more `live-server`.

4. **Taxonomies** - Auto tag pages `/tags/csharp/` from frontmatter tags.

5. **i18n** - `/en/posts` `/id/posts` from folder.

We keep philosophy: No Node, pure .NET, minimal config.

Want contribute? PR to `axcora/dax` - it's MIT.
