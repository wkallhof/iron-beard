using System;
using System.IO;
using System.Threading.Tasks;
using Raud.Core.Features.FileSystem;
using Raud.Core.Features.Shared;

namespace Raud.Core.Features.Razor
{
    public class RazorFileProcessor : IFileProcessor
    {
        private IFileSystem _fileSystem;
        private string _inputDirectory;
        private RazorViewRenderer _renderer;

        public RazorFileProcessor(IFileSystem fileSystem, string inputDirectory){
            this._fileSystem = fileSystem;
            this._inputDirectory = inputDirectory;
            this._renderer = new RazorViewRenderer(inputDirectory);
        }

        public async Task<(bool processed, OutputFile file)> ProcessInputAsync(InputFile file, string outputDirectory)
        {
            if (!file.Extension.ToLower().Equals(".cshtml") || file.Name.StartsWith("_"))
                return (false, null);

            Console.WriteLine($"[Razor] Processing Input : {Path.Combine(file.RelativeDirectory, file.Name + file.Extension)}");

            var relativeFile = Path.Combine(file.RelativeDirectory, file.Name + file.Extension);
            var html = await this._renderer.RenderAsync(relativeFile, "test");

            var output = OutputFile.FromInputFile(file);
            output.Content = html;
            output.Extension = ".html";
            output.FullDirectory = Path.GetFullPath(outputDirectory + output.RelativeDirectory);
            output.FullPath = Path.Combine(output.FullDirectory, output.Name + output.Extension);

            return (true, output);
        }

        public async Task ProcessOutputAsync(OutputFile file){
            if(!file.Input.Extension.ToLower().Equals(".md"))
                return;

            Console.WriteLine($"[Razor] Processing Output : {Path.Combine(file.RelativeDirectory, file.Name + file.Extension)}");

            var layoutTemplate = "@{ Layout = \"../Shared/_Layout.cshtml\"; }\n";
            var html = layoutTemplate + file.Content;
            var tempFile = await this._fileSystem.CreateTempFileAsync(html);

            var relativeFile = tempFile.Replace(this._inputDirectory, "");
            var razorHtml = await this._renderer.RenderAsync(relativeFile, "test");
            file.Content = razorHtml;
        }
    }
}