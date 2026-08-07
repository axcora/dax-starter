---
title: Markdig Power: Extended Markdown in DAX .NET SSG
description: How DAX uses Markdig to support tables, footnotes, code highlighting, and custom extensions
image: /img/dax-primary-logo.webp
layout: post.dax
date: 2026-08-03
tags: [markdown, markdig, csharp, content]
---

DAX uses Markdig - the most powerful Markdown parser in .NET (used by DocFX).

Supports:

- Tables, Task Lists, Footnotes
- Code fence with C# highlighting
- YAML frontmatter natively
- Auto TOC, Emoji, Custom containers

Example:

```markdown
::: tip
DAX builds this in 12ms!
:::

| Feature | DAX | Jekyll |
|---------|-----|--------|
| Speed | 0.3s | 40s |
```

Code highlight auto via CSS, no JS.

You write Markdown, DAX ships HTML. Fast.
