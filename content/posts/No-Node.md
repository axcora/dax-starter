---
title: No Node.js, No npm Hell: Building SSG Pure C#
description: Why DAX removes Node.js dependency and how it improves CI/CD and developer experience
image: /img/icon.webp
layout: post.dax
date: 2026-08-05
tags: [dotnet, nodejs, devops, csharp]
---

Every JS SSG starts with `npm install` 400MB and 3 minutes CI.

DAX starts with:

```bash
dotnet run -- build
```

That's it. .NET SDK is already in GitHub Actions `ubuntu-latest`.

### CI/CD Speed

Our GitHub Actions workflow finishes in 28 seconds including restore.

Astro? 2m 15s. Next.js? 4m.

No `node_modules`, no `package-lock.json` conflict.

### Developer Experience

You already know C#. Why learn Go template or Liquid weird syntax?

DAX layout is simple HTML with `{{ title }}` and `{{ content }}` - Razor-lite.

C# devs feel home instantly.
