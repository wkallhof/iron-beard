using System;
using System.IO;
using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Shared;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Logging;

namespace IronBeard.Core.Features.Razor
{
    public class RazorFileProcessor : IProcessor
    {
        private IFileSystem _fileSystem;
        private ILogger _log;
        private RazorViewRenderer _renderer;

        public RazorFileProcessor(IFileSystem fileSystem, ILogger logger, string inputDirectory){
            this._fileSystem = fileSystem;
            this._log = logger;
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

            this._log.Info($"[Razor] Processing Input : {file.RelativePath}");

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
            var viewContext = new ViewContext(file, context);
            this._log.Info($"[Razor] Processing Markdown Output : { file.RelativePath }");
            file.Content = await this.CreateTempAndRender(file.Content, file, context, viewContext);
        }

        private async Task ProcessRazor(OutputFile file, GeneratorContext context){
            this._log.Info($"[Razor] Processing Razor Output : { file.Input.RelativePath }");
            var viewContext = new ViewContext(file, context);
            var fileContent = await this._fileSystem.ReadAllTextAsync(file.Input.FullPath);
            if(!fileContent.IsSet())
                return;

            file.Content = await this.CreateTempAndRender(fileContent, file.Input, context, viewContext);
        }

        private async Task<string> CreateTempAndRender(string fileContent, InputFile file, GeneratorContext genContext, ViewContext viewContext){
            var html = this.AppendLayoutInfo(fileContent, genContext.Layout);
            var tempFile = await this._fileSystem.CreateTempFileAsync(html);
            try{
                return await this._renderer.RenderAsync(tempFile.RelativePath, viewContext);
            }
            catch(Exception e)
            {
                var message = e.Message.Replace(tempFile.FullPath, file.FullPath);
                this._log.Error(message);
                return string.Empty;
            }
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