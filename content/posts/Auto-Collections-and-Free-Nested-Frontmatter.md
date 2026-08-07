---
title: Auto Collections and Free Nested Frontmatter - Jekyll Style but Faster
description: How DAX auto collections and unlimited nested YAML frontmatter works without fixed schema
image: /img/favicon.webp
layout: post.dax
date: 2026-08-02
tags: [tutorial, collections, frontmatter, jekyll]
---

Jekyll locks you to `_posts` and fixed schema. DAX? Free form.

In DAX you can do:

```yaml
---
title: My Post
hero:
  title1: DAX
  title2: Fast
  bento:
    item:
      - title: nested
        deep:
          level3: as deep as you want
customField: whatever you want
mySquad:
  - name: Axcora
    role: Maintainer
---
```

No `_config.yml` hell. DAX deserializes YAML to `dynamic` ExpandoObject in C#.

### Auto Collections

Folder name = collection name.

- `/posts/*.md` -> `site/posts/`
- `/projects/*.md` -> `site/projects/`
- `/docs/api/*.md` -> `site/docs/api/`

Auto pagination, auto `collection.posts` in layout. Zero config.

This is Jekyll done right, but in C# speed.
