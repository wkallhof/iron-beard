# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

IronBeard is a cross-platform static site generator built with .NET 10. It processes Razor `.cshtml` and Markdown `.md` files into static HTML, preserving folder structure and copying static assets. Distributed as a .NET global tool (`dotnet tool install --global IronBeard`).

## Build & Run Commands

```bash
dotnet build ./src                    # Build solution
make example                          # Generate static site from sample project
make watch                            # Watch sample project and rebuild on changes
make serve                            # Serve generated output (requires dotnet-serve)
```

The solution file is `src/IronBeard.slnx`. There are no unit tests — CI validates by running `generate` against the sample project:
```bash
dotnet run --project ./src/IronBeard.Cli -- generate -i ./samples/razor-markdown-sample
```

## Architecture

Two projects in `src/`:
- **IronBeard.Cli** — Entry point, CLI commands (`generate`, `watch`), DI setup, console logging
- **IronBeard.Core** — All processing logic, shared interfaces, data models

### Processor Pipeline

The core architecture is a pipeline of processors implementing `IProcessor` (in `Features/Shared/IProcessor.cs`). Each processor has three lifecycle phases: `PreProcessAsync`, `ProcessAsync`, `PostProcessAsync`.

**StaticGenerator** (`Features/Generator/StaticGenerator.cs`) orchestrates the pipeline, running all four processors in order:

1. **MarkdownProcessor** — Extracts YAML frontmatter, converts `.md` to HTML via Markdig
2. **RazorProcessor** — Handles `.cshtml` files, wraps content in `_Layout.cshtml`, renders via ASP.NET Core Razor engine. Creates temporary `.cshtml` files on disk for the Razor engine.
3. **StaticProcessor** — Copies static assets (CSS, JS, images) directly to output
4. **HtmlFormatProcessor** — Reformats output HTML via XML parsing

### Key Data Flow

`InputFile` (scanned source) → Processors → `OutputFile` (rendered content + metadata + URL)

**GeneratorContext** holds state across the pipeline (layouts, all input/output files, directory paths). **ViewContext** is the model passed to Razor templates, providing access to `Current`, `Siblings`, `Children`, and `All` pages.

### DI Setup

Dependency injection is configured in `GenerateCommand.cs` — this is where processors are registered and the generator is wired up.

### Metadata System

- Markdown: YAML frontmatter between `---` delimiters at file start
- Razor: YAML between `@*META` and `*@` comments (can appear anywhere in file)
- Accessed in templates via `Model.Current.Metadata` dictionary

### Configuration

`beard.json` in project root. Key settings: `SiteTitle`, `IndexFileName` (default: "index"), `LayoutFileName` (default: "_Layout"), `StaticExtensionIgnoreList`, `ExcludeHtmlExtension` (default: true), `EnableMarkdownExtensions`. Model is `BeardConfig` in `Features/Configuration/`.

## CI/CD

GitHub Actions (`.github/workflows/release.yml`): builds, runs generate on sample, packs NuGet, pushes to nuget.org. Triggers on push to `master`.
