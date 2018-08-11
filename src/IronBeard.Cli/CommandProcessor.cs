using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Markdown;
using IronBeard.Core.Features.Razor;
using IronBeard.Core.Features.Static;
using ColorConsole = Colorful.Console;

namespace IronBeard.Cli
{
    public class CommandProcessor
    {
        private ProgressBar _progressBar;

        public async Task Process(string[] args){
            //Console.WriteLine("IronBeard Generator -- Static Site Build");

            var inputArg = args.ElementAtOrDefault(0) ?? ".";
            var outputArg = args.ElementAtOrDefault(1) ?? Path.Combine(inputArg, "output");

            var inputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, inputArg));
            var outputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputArg));

            //Console.WriteLine("Input Directory: " + inputPath);
            //Console.WriteLine("Output Directory: " + outputPath);

            var fileSystem = new DiskFileSystem();

            var generator = new StaticGenerator(fileSystem, inputPath, outputPath);
            generator.AddProcessor(new MarkdownFileProcessor(fileSystem));
            generator.AddProcessor(new RazorFileProcessor(fileSystem, inputPath));
            generator.AddProcessor(new StaticFileProcessor(ignore: new List<string> { ".cshtml", ".md", ".DS_Store" }));

            generator.OnProgress += OnGeneratorProgress;
            generator.OnError += OnGeneratorError;
            generator.OnInfo += OnGeneratorInfo;

            using (this._progressBar = new ProgressBar()) {
                await generator.Generate();
            }

            ColorConsole.WriteLine("Complete!", Color.Green);
        }

        private void OnGeneratorProgress(object sender, EventArgs e)
        {
            var progressArgs = (OnProgressEventArgs)e;
            this._progressBar.Report((double) progressArgs.Percent / 100, progressArgs.Message);
            //Console.WriteLine(progressArgs.Message);
        }

        private void OnGeneratorError(object sender, EventArgs e)
        {
            var errorArgs = (OnErrorEventArgs)e;
            Console.WriteLine(errorArgs.Error);
        }

        private void OnGeneratorInfo(object sender, EventArgs e)
        {
            var infoArgs = (OnInfoEventArgs)e;
            this._progressBar.MessageBefore(infoArgs.Message);
            //Console.WriteLine(infoArgs.Message);
        }
    }
}