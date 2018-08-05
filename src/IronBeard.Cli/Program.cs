using System;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Extensions;
using System.IO;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Markdown;
using IronBeard.Core.Features.Razor;
using IronBeard.Core.Features.Static;
using System.Collections.Generic;

namespace IronBeard.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("IronBeard Generator -- Static Site Build");

            var inputArg = args.ElementAtOrDefault(0) ?? ".";
            var outputArg = args.ElementAtOrDefault(1) ?? Path.Combine(inputArg, "output");

            var inputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, inputArg));
            var outputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputArg));

            Console.WriteLine("Input Directory: " + inputPath);
            Console.WriteLine("Output Directory: " + outputPath);

            var fileSystem = new DiskFileSystem();

            var generator = new IronBeardGenerator(fileSystem, inputPath, outputPath);
            generator.AddProcessor(new MarkdownFileProcessor(fileSystem));
            generator.AddProcessor(new RazorFileProcessor(fileSystem, inputPath));
            generator.AddProcessor(new StaticFileProcessor(ignore: new List<string> { ".cshtml", ".md", ".DS_Store" }));

            await generator.Generate();
        }
    }
}
