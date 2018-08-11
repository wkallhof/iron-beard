using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Cli.Features.Logging;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Markdown;
using IronBeard.Core.Features.Razor;
using IronBeard.Core.Features.Static;

namespace IronBeard.Cli.Features.Commands
{
    public class CommandProcessor
    {
        private ILogger _log;

        public CommandProcessor(ILogger logger){
            this._log = logger;
        }

        public async Task Process(string[] args){
            this._log.Info("IronBeard Generator -- Static Site Build");

            var inputArg = args.ElementAtOrDefault(0) ?? ".";
            var outputArg = args.ElementAtOrDefault(1) ?? Path.Combine(inputArg, "output");

            var inputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, inputArg));
            var outputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputArg));

            var fileSystem = new DiskFileSystem(this._log);

            var generator = new StaticGenerator(fileSystem, this._log, inputPath, outputPath);
            generator.AddProcessor(new MarkdownFileProcessor(fileSystem, this._log));
            generator.AddProcessor(new RazorFileProcessor(fileSystem, this._log, inputPath));
            generator.AddProcessor(new StaticFileProcessor(this._log, ignore: new List<string> { ".cshtml", ".md", ".DS_Store" }));

            var startTime = DateTime.Now;
            await generator.Generate();
            var endTime = DateTime.Now;
            var diff = endTime - startTime;

            this._log.Info($"Completed in {diff.TotalSeconds} seconds.");
        }
    }
}