---
layout: home.dax
title: DAX - The Blazing Fast .NET Static Site Generator
description: "DAX is a minimal, dependency-free SSG built with C# .NET 8. No Node.js, no bloat, pure static HTML in milliseconds."

hero:
  title1: DAX
  title2: .NET Static, No Bullshit.
  text: The C# SSG that builds 500 pages in <100ms. Pure .NET 8, pure HTML, zero Node.js.

bento1:
  title: dotnet run -- build. Done.
  text: No npm install, no webpack hell. Just C#.
  image: /img/daxlogo.webp
  article:
    title: Why C# for SSG?
    info:
      - text: Node.js SSG is slow and bloated. DAX uses .NET 8 native compilation. Build 1000 markdown files faster than Astro can start its dev server.
      - text: Inspired by Jekyll, Eleventy, Hugo — but rewritten in C#. Familiar frontmatter, layouts, collections, but with the speed and safety of .NET.
  button:
    text: Download DAX .NET Starter
    url: https://github.com/axcora/dax

bento2:
  item:
    - title: C# Native Collections
      text: Folder = Collection. DAX auto-scans /posts, /projects, /docs and generates pagination, archives, and taxonomies. No _config.yml nightmare.
      image: /img/favicon.webp
    - title: Unlimited Nested Frontmatter
      text: YAML frontmatter without schema limit. Nest hero.bento.blog.start as deep as you want. Jekyll style, but strongly-typed in C#.
      image: /img/icon.webp

bento3:
  item:
    - icon: /img/icon/fast.svg
      title: 0ms Cold Start
      text: No Node. No JS runtime. dotnet run is the builder. CI/CD builds finish before Vercel even installs dependencies.
    - icon: /img/icon/champ.svg
      title: Razor-ish DAX Layout
      text: .dax layout files - simple, familiar, like Liquid but without the weird syntax. Pure C# string templating.
    - icon: /img/icon/seo.svg
      title: SEO Out of The Box
      text: Auto sitemap.xml, rss.xml, canonical, OpenGraph. DAX generates clean semantic HTML that Google loves.
    - icon: /img/icon/md.svg
      title: Deploy Anywhere
      text: Output is /site folder - pure static. Deploy to GitHub Pages, Cloudflare Pages, Netlify, or your own VPS with DAX domain.

blog:
  title: DAX Engineering Logs
  button:
    text: Explore All DAX Articles
    url: /posts/

start:
  title: Start with DAX in 30s
  text: dotnet new + dax = your site live. No tutorial hell.
  image: /img/dax-primary-logo.webp
  button:
    text: Get DAX Starter Zip
    url: https://github.com/axcora/dax

footer:
  icon: /img/icon/contact.svg
  title: Built with .NET 8
  text: DAX is open source by AXcora. Fork it, hack it, ship it.
  item:
    - icon: /img/icon/address.svg
      title: Open Source
      text: MIT Licensed on GitHub
    - icon: /img/icon/chat.svg
      title: C# Community
      text: Join discussions on GitHub
    - icon: /img/icon/email.svg
      title: Docs
      text: Check DAX Wiki & README
---
