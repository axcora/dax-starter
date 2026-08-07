---
title: Understanding DAX .dax Layout Engine - Razor Lite for Static
description: Deep dive into DAX layout system, how .dax files work vs Liquid, Razor, and Nunjucks
image: /img/dax-primary-logo.webp
layout: post.dax
date: 2026-08-02
tags: [layout, razor, templating, dax]
---

DAX doesn't use Razor (too heavy). It uses custom `.dax` engine.

Example `layout/post.dax`:

```html
<html>
<head><title>{{ title }}</title></head>
<body>
  <h1>{{ title }}</h1>
  <div>{{ content }}</div>
  {{#if tags}}
    <ul>{{#each tags}}<li>{{ this }}</li>{{/each}}</ul>
  {{/if}}
</body>
</html>
```

Simple Mustache-like syntax, but powered by C#.

No `@Model`, no `@using`, no compilation error hell. Just string replace + Markdown rendering via Markdig.

You can nest layouts: `layout: post.dax` which itself uses `layout: base.dax`.

Fast, debuggable, no magic.
