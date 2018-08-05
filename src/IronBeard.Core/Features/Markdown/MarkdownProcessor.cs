using System;
using System.IO;
using System.Threading.Tasks;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Shared;


namespace IronBeard.Core.Features.Markdown
{
    public class MarkdownFileProcessor : IProcessor
    {
        private IFileSystem _fileSystem;

        public MarkdownFileProcessor(IFileSystem fileSystem){
            this._fileSystem = fileSystem;
        }

        public Task BeforeProcessAsync(InputFile file, GeneratorContext context) => Task.CompletedTask;

        public async Task<OutputFile> ProcessAsync(InputFile file, GeneratorContext context)
        {
            if (!file.Extension.ToLower().Equals(".md"))
                return null;

            Console.WriteLine($"[Markdown] Processing Input: {file.RelativePath}");

            var markdown = await this._fileSystem.ReadAllTextAsync(file.FullPath);
            if (!markdown.IsSet())
                return null;

            var html = Markdig.Markdown.ToHtml(markdown);
            var output = OutputFile.FromInputFile(file);
            output.Content = html;
            output.Extension = ".html";
            output.BaseDirectory = context.OutputDirectory;

            return output;
        }

        public Task AfterProcessAsync(OutputFile file, GeneratorContext context) => Task.CompletedTask;
    }
}