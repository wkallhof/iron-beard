using System.IO;
using System.Threading.Tasks;
using Raud.Core.Extensions;
using Raud.Core.Features.FileSystem;
using Raud.Core.Features.Shared;


namespace Raud.Core.Features.Markdown
{
    public class MarkdownFileProcessor : IFileProcessor
    {
        private IFileSystem _fileSystem;

        public MarkdownFileProcessor(IFileSystem fileSystem){
            this._fileSystem = fileSystem;
        }

        public async Task<(bool processed, OutputFile file)> ProcessInputAsync(InputFile file, string outputDirectory)
        {
            if (!file.Extension.ToLower().Equals(".md"))
                return (false, null);

            var markdown = await this._fileSystem.ReadAllTextAsync(file.FullPath);
            if (!markdown.IsSet())
                return (false, null);

            var html = Markdig.Markdown.ToHtml(markdown);
            var output = OutputFile.FromInputFile(file);
            output.Content = html;
            output.Extension = ".html";
            output.FullDirectory = Path.GetFullPath(outputDirectory + output.RelativeDirectory);
            output.FullPath = Path.Combine(output.FullDirectory, output.Name + output.Extension);

            return (true, output);
        }

        public Task ProcessOutputAsync(OutputFile file) => Task.CompletedTask;
    }
}