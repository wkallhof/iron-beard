using System;
using System.IO;
using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Shared;
using IronBeard.Core.Extensions;

namespace IronBeard.Core.Features.Razor
{
    public class RazorFileProcessor : IProcessor
    {
        private IFileSystem _fileSystem;
        private RazorViewRenderer _renderer;

        public RazorFileProcessor(IFileSystem fileSystem, string inputDirectory){
            this._fileSystem = fileSystem;
            this._renderer = new RazorViewRenderer(inputDirectory);
        }

        public Task BeforeProcessAsync(InputFile file, GeneratorContext context)
        {
            // set the layout if it is found
            //TODO: Consider how to support multiple layouts
            if(this.IsCshtmlFile(file) && file.Name.Equals("_Layout", StringComparison.OrdinalIgnoreCase))
                context.Layout = file;

            return Task.CompletedTask;
        }

        public Task<OutputFile> ProcessAsync(InputFile file, GeneratorContext context)
        {
            // if this isn't CSHTML, or this is a Layout, or a partial, ignore
            if (!this.IsCshtmlFile(file) || context.Layout.Equals(file) || file.Name.StartsWith("_"))
                return Task.FromResult<OutputFile>(null);

            Console.WriteLine($"[Razor] Processing Input : {file.RelativePath}");

            var output = OutputFile.FromInputFile(file);
            output.Extension = ".html";
            output.BaseDirectory = context.OutputDirectory;

            return Task.FromResult(output);
        }

        public async Task AfterProcessAsync(OutputFile file, GeneratorContext context)
        {
            if(file.Input.Extension.ToLower().Equals(".md"))
                await this.ProcessMarkdown(file, context);

            if(this.IsCshtmlFile(file.Input) && !context.Layout.Equals(file.Input) && !file.Input.Name.StartsWith("_"))
                await this.ProcessRazor(file, context);
        }

        private async Task ProcessMarkdown(OutputFile file, GeneratorContext context){
            Console.WriteLine($"[Razor] Processing Markdown Output : { file.RelativePath }");
            file.Content = await this.CreateTempAndRender(file.Content, context);
        }

        private async Task ProcessRazor(OutputFile file, GeneratorContext context){
            Console.WriteLine($"[Razor] Processing Razor Output : { file.Input.RelativePath }");

            var fileContent = await this._fileSystem.ReadAllTextAsync(file.Input.FullPath);
            if(!fileContent.IsSet())
                return;

            file.Content = await this.CreateTempAndRender(fileContent, context);
        }

        private async Task<string> CreateTempAndRender(string fileContent, GeneratorContext context){
            var html = this.AppendLayoutInfo(fileContent, context.Layout);
            var tempFile = await this._fileSystem.CreateTempFileAsync(html);
            return await this._renderer.RenderAsync(tempFile.RelativePath, context);  
        }

        private string AppendLayoutInfo(string fileContent, InputFile layout){
            if(layout == null)
                return fileContent;

            var relativePath = Path.Combine("~", layout.RelativePath);

            var razorLayoutString = $"@{{ Layout = \"{ relativePath }\"; }}\n";
            return razorLayoutString + fileContent;
        }

        private bool IsCshtmlFile(InputFile file){
            return file.Extension.Equals(".cshtml", StringComparison.Ordinal);
        }
    }
}