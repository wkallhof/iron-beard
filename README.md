# IronBeard
[![NuGet][nuget-badge]][nuget] [![Build status][appveyor-badge]][appveyor] 

[nuget]: https://www.nuget.org/packages/IronBeard/
[nuget-badge]: https://img.shields.io/nuget/v/IronBeard.svg?style=flat-square&label=nuget
[appveyor]: https://ci.appveyor.com/project/wkallhof/iron-beard/branch/master
[appveyor-badge]: https://ci.appveyor.com/api/projects/status/xf9ra9257yclw3gg/branch/master?svg=true

A simple and easy to use cross-platform static site generator built with .NET Core

## Features
- [x] Support for recursive folder and file structures
- [x] Markdown Processor
- [x] Razor Processor
- [x] Static File Processor
- [x] Razor Layout Support (wraps other razor files and markdown markup)
- [x] Markdown metadata (YAML Frontmatter support in markdown)
- [x] Razor metadata (YAML Frontmatter support with Razor comments)
- [x] HTML Formatting to clean up file output. 
- [x] URL correction (properly handles relative routes and root folder routing (index.html etc.))
- [x] Global configuration file
- [x] Rich CLI output
- [x] Valid system errors codes (useful for automation)
- [x] Watch command for automatic rebuilding on file or directory change
- [ ] Project Scaffolding
- [ ] Static Web Server


## Get started

Download the [.NET Core 2.1 SDK](https://aka.ms/DotNetCore21) or newer.
Once installed, run this command:

```
dotnet tool install --global IronBeard
```
This will install the `beard` command globally on your machine.

The simplest way to build a static site is by running the following in your project directory

```
beard
```

It will generate a `www` folder in your current directory with the static site files

## Usage

```
Usage: beard [options] [command]

Options:
  --version     Show version information
  -?|-h|--help  Show help information

Commands:
  generate      Generates a static site from the files in the given directory
  watch         Watch a directory for changes and rebuild automatically
```

## Generate

```
Generates a static site from the files in the given directory

Usage: beard generate [options]

Options:
  -i|--input <PATH>   Provide the root directory where Iron Beard should look for files to generate a static site from.
  -o|--output <PATH>  Provide the directory where Iron Beard should write the static site to.
  -?|-h|--help        Show help information
```

## Watch
```
Watch a directory for changes and rebuild automatically

Usage: beard watch [options]

Options:
  -i|--input <PATH>   Provide the root directory where Iron Beard should look for files to generate a static site from.
  -o|--output <PATH>  Provide the directory where Iron Beard should write the static site to.
  -?|-h|--help        Show help information
```

