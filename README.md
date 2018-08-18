# IronBeard
[![NuGet][nuget-badge]][nuget] [![Build status][appveyor-badge]][appveyor] 

[nuget]: https://www.nuget.org/packages/IronBeard/
[nuget-badge]: https://img.shields.io/nuget/v/IronBeard.svg?style=flat-square&label=nuget
[appveyor]: https://ci.appveyor.com/project/wkallhof/iron-beard/branch/master
[appveyor-badge]: https://ci.appveyor.com/api/projects/status/xf9ra9257yclw3gg/branch/master?svg=true

A simple and easy to use cross-platform static site generator built with .NET Core. IronBeard processes your Razor `.cshtml` files, markdown `.md` files into full `.html` files ready for static hosting on services like Amazon S3.

IronBeard maintains your folder structure and copies static assets like images, JS, and CSS into their respective directories to maintain the correct linking on the generated site.

Adding a `beard.json` file to your project root allows for further configuration (see below).

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

It will scan your current directory for site files and generate a `www` folder in your current directory with the generated static site.

## Example

See the [Samples](./samples) directory for sample projects that can be built with IronBeard.

### Example Structure

```
.
├── beard.json                  # IronBeard configuration file in the root
├── index.cshtml                # Main homepage file
├── shared                      # Standard Shared folder, common in .NET templating
│   ├── _Layout.cshtml          # Standard _Layout.cshtml file
│   ├── Partials                # Full Partials support
|   |   └── ...
│   └── ...
├── articles                    # Any level of folder testing
│   ├── foo-bar.md              # Markdown file support
│   ├── lorem-ipsum.cshtml      # Razor File support for more complex pages
│   └── ...
├── assets                      # Standard assets folder structure. Include CSS, JS, Images, etc.
│   ├── site.css                
│   ├── site.js
│   ├── images
|   |   └── ...
│   └── ...        
└── ...
```

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
Generate is the main and default command for IronBeard. This will take in your provided input folder (defaults to the current directory) and generate your static site into the provided output folder (defaults to `./www`);

```
Generates a static site from the files in the given directory

Usage: beard generate [options]

Options:
  -i|--input <PATH>   Provide the root directory where Iron Beard should look for files to generate a static site from.
  -o|--output <PATH>  Provide the directory where Iron Beard should write the static site to.
  -?|-h|--help        Show help information
```

## Watch
Watch is similary to Generate (the paramaters are all the same), but once it is done generating, it will continue to watch your input directory for changes. When any changes are detected, it will automatically re-generate the static site.
```
Watch a directory for changes and rebuild automatically

Usage: beard watch [options]

Options:
  -i|--input <PATH>   Provide the root directory where Iron Beard should look for files to generate a static site from.
  -o|--output <PATH>  Provide the directory where Iron Beard should write the static site to.
  -?|-h|--help        Show help information
```

## Configuration
IronBeard allows for further configuration by adding a `beard.json` configuration file in the root of your project.
The default configuration is as follows:

```
{
    "Config" : {
        "SiteTitle" : "Razor Markdown Sample",
        "IndexFileName" : "index",
        "LayoutFileName" : "_Layout",
        "StaticExtensionIgnoreList" : [".cshtml", ".md", ".DS_Store", ".json" ]
    }
}
```

* `SiteTitle` : This is the title to display for your generated site. This will be propagated to things like the browser tab.

* `IndexFileName` : This is the file name that should be display as the root in any directory. For example, if you had a `/projects` folder and you wanted a page to represent your projects, you'd put a file `/projects/index.cshtml` into that diretory, which will be loaded when a user goes to `/projects` in their browser

* `LayoutFileName` : This is the layout file used to wrap your `.cshtml` and `.md` files. IronBeard will look for this file to determine the layout to use.

* `StaticExtensionIgnoreList` : This array should hold the list of extensions you want the static processor to ignore. If it is _not_ in this list, the files will be copied into the output directory.
