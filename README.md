# IronBeard

[![NuGet](https://img.shields.io/nuget/v/IronBeard.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/IronBeard/)
[![Release Status](https://github.com/wkallhof/iron-beard/actions/workflows/release.yml/badge.svg)](https://github.com/wkallhof/iron-beard/actions/workflows/release.yml)

A cross-platform static site generator built with .NET. IronBeard processes Razor (`.cshtml`) and Markdown (`.md`) files into static HTML ready for hosting on services like Amazon S3, GitHub Pages, or any static file server.

- Preserves your folder structure and copies static assets (images, JS, CSS) to maintain correct linking
- Wraps content in shared Razor layouts
- Supports YAML metadata in both Markdown and Razor files
- Cleans up generated HTML output
- Handles URL routing automatically (index files, clean URLs without `.html` extensions)
- Watches for file changes and rebuilds automatically

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) or newer

### Installation

```bash
dotnet tool install --global IronBeard
```

This installs the `beard` command globally on your machine.

### Quick Start

Run `beard` in your project directory to scan for site files and generate a static site into a `www` folder:

```bash
beard
```

## Usage

```
beard [options] [command]

Commands:
  generate    Generate a static site from the files in a directory (default)
  watch       Watch a directory for changes and rebuild automatically

Options:
  --version       Show version information
  -?|-h|--help    Show help information
```

### `generate`

The default command. Takes an input folder (defaults to current directory) and generates your static site into an output folder (defaults to `./www`).

```bash
beard generate -i ./my-site -o ./output
```

| Option | Description |
|---|---|
| `-i\|--input <PATH>` | Root directory to scan for source files (default: current directory) |
| `-o\|--output <PATH>` | Directory to write the generated site to (default: `./www`) |

### `watch`

Same options as `generate`, but continues watching the input directory for changes and automatically rebuilds when files are modified.

```bash
beard watch -i ./my-site -o ./output
```

### Serving Locally

For local preview, use [dotnet-serve](https://github.com/natemcmaster/dotnet-serve):

```bash
dotnet tool install --global dotnet-serve
dotnet serve ./www
```

## Project Structure

See the [samples](./samples) directory for a complete working example.

```
.
├── beard.json                  # Configuration file (optional)
├── index.cshtml                # Homepage
├── shared/
│   ├── _Layout.cshtml          # Layout template wrapping all pages
│   └── Partials/
│       └── _navigation.cshtml  # Reusable partial views
├── articles/
│   ├── index.cshtml            # Section landing page
│   ├── my-post.md              # Markdown content
│   └── my-page.cshtml          # Razor content
└── assets/
    ├── site.css                # Static assets are copied as-is
    ├── site.js
    └── images/
```

## Configuration

Add a `beard.json` file to your project root to customize behavior:

```json
{
    "Config": {
        "SiteTitle": "My Site",
        "IndexFileName": "index",
        "LayoutFileName": "_Layout",
        "StaticExtensionIgnoreList": [".cshtml", ".md", ".DS_Store", ".json"],
        "ExcludeHtmlExtension": true,
        "EnableMarkdownExtensions": false
    }
}
```

| Option | Default | Description |
|---|---|---|
| `SiteTitle` | — | Site title displayed in the browser tab |
| `IndexFileName` | `"index"` | Filename treated as the directory index (e.g., `/articles/index.cshtml` serves at `/articles`) |
| `LayoutFileName` | `"_Layout"` | Name of the layout file used to wrap `.cshtml` and `.md` content |
| `StaticExtensionIgnoreList` | `[".cshtml", ".md", ".DS_Store", ".json"]` | File extensions the static processor should skip (everything else is copied to output) |
| `ExcludeHtmlExtension` | `true` | Omit `.html` extensions from output files for cleaner URLs (`/articles/my-post` instead of `/articles/my-post.html`). Your static host may need to be configured to serve these files with `content-type: text/html`. Set to `false` to include `.html` extensions. |
| `EnableMarkdownExtensions` | `false` | Enable [Markdig extensions](https://github.com/lunet-io/markdig) (tables, abbreviations, etc.) |

## Metadata

IronBeard supports YAML metadata in both Markdown and Razor files. Metadata is exposed to Razor templates via the `Metadata` dictionary on each page's `OutputFile` model.

### In Markdown

Standard YAML frontmatter at the top of the file:

```markdown
---
Title: My Article
Tags: Blog
---

# Article content here
```

### In Razor

Uses a special Razor comment syntax. Can appear anywhere in the file:

```html
@*META
Title: My Article
Tags: Blog
*@
```

## ViewContext

Razor files receive a `ViewContext` model with access to the current page and its relationship to other pages. Add this to the top of your Razor file:

```html
@model IronBeard.Core.Features.Generator.ViewContext
```

| Property | Type | Description |
|---|---|---|
| `Model.Current` | `OutputFile` | The current page, including its `Metadata` dictionary |
| `Model.Siblings` | `IEnumerable<OutputFile>` | Other pages in the same directory |
| `Model.Children` | `IEnumerable<OutputFile>` | Pages in subdirectories |
| `Model.All` | `IEnumerable<OutputFile>` | All HTML pages in the site |

### Example: Listing Articles

```html
@using System.Linq
@model IronBeard.Core.Features.Generator.ViewContext
@{
    var articles = Model.Siblings.Where(x => x.Metadata.ContainsKey("Title"));
}

@if(articles.Any())
{
    <h2>Articles</h2>
    <ul>
        @foreach(var article in articles){
            <li><a href="@article.Url">@article.Metadata["Title"]</a></li>
        }
    </ul>
}
```

## Partials

Partials are fully supported. Partial paths must be relative to the project root, not the current file:

```html
<!-- Good -->
<partial name="/shared/partials/_articles.cshtml" />

<!-- Bad — don't use relative paths -->
<partial name="../../shared/partials/_articles.cshtml" />
```

## License

[MIT](./LICENSE)
