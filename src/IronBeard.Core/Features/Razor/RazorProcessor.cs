using System;
using System.IO;
using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Shared;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Logging;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Configuration;

namespace IronBeard.Core.Features.Razor
{
    public class RazorProcessor : IProcessor
    {

        private IFileSystem _fileSystem;
        private ILogger _log;
        private IUrlProvider _urlProvider;
        private BeardConfig _config;
        private RazorViewRenderer _renderer;
        private GeneratorContext _context;

        private const string YAML_DEL_START = "@*META";
        private const string YAML_DEL_END = "*@";

        public RazorProcessor(IFileSystem fileSystem, RazorViewRenderer renderer, 
            ILogger logger, IUrlProvider urlProvider, BeardConfig config, GeneratorContext context){
            this._fileSystem = fileSystem;
            this._log = logger;
            this._urlProvider = urlProvider;
            this._config = config;
            this._renderer = renderer;
            this._context = context;
        }

        public Task PreProcessAsync(InputFile file)
        {
            if(this.IsCshtmlFile(file) && file.Name.IgnoreCaseEquals(this._config.LayoutFileName))
                this._context.Layout = file;

            return Task.CompletedTask;
        }

        public async Task<OutputFile> ProcessAsync(InputFile file)
        {
            // if this isn't CSHTML, or this is a Layout, or a partial, ignore
            if (!this.IsCshtmlFile(file) || this._context.Layout.Equals(file) || file.Name.StartsWith("_"))
                return null;

            this._log.Info<RazorProcessor>($"Processing Input : {file.RelativePath}");

            var fileContent = await this._fileSystem.ReadAllTextAsync(file.FullPath);
            if(!fileContent.IsSet())
                return null;

            var metadata = this.ExtractYamlMetadata(fileContent);

            var output = OutputFile.FromInputFile(file);
            output.Extension = ".html";
            output.BaseDirectory = this._context.OutputDirectory;
            output.Metadata = metadata;
            output.Url = this._urlProvider.GetUrl(file);

            return output;
        }

        public async Task PostProcessAsync(OutputFile file)
        {
            if(file.Input.Extension.IgnoreCaseEquals(".md"))
                await this.ProcessMarkdown(file);

            if(this.IsCshtmlFile(file.Input) && !this._context.Layout.Equals(file.Input) && !file.Input.Name.StartsWith("_"))
                await this.ProcessRazor(file);
        }

        private async Task ProcessMarkdown(OutputFile file){
            var viewContext = new ViewContext(file, this._context);
            this._log.Info<RazorProcessor>($"Processing Markdown Output : { file.RelativePath }");
            file.Content = await this.CreateTempAndRender(file.Content, file, viewContext);
        }

        private async Task ProcessRazor(OutputFile file){
            this._log.Info<RazorProcessor>($"Processing Razor Output : { file.Input.RelativePath }");
            var viewContext = new ViewContext(file, this._context);
            var fileContent = await this._fileSystem.ReadAllTextAsync(file.Input.FullPath);
            if(!fileContent.IsSet())
                return;

            file.Content = await this.CreateTempAndRender(fileContent, file.Input, viewContext);
        }

        private async Task<string> CreateTempAndRender(string fileContent, InputFile file, ViewContext viewContext){
            if(this._renderer == null){
                this._log.Fatal<RazorProcessor>("Renderer has not been initialized. Must call InitializeViewRenderer before processing");
                return string.Empty;
            }

            var html = this.AppendLayoutInfo(fileContent, this._context.Layout);
            var tempFile = await this._fileSystem.CreateTempFileAsync(html);
            try{
                return await this._renderer.RenderAsync(tempFile.RelativePath, viewContext);
            }
            catch(Exception e)
            {
                var message = e.Message.Replace(tempFile.FullPath, file.FullPath);
                this._log.Error<RazorProcessor>(message);
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

        private Dictionary<string,string> ExtractYamlMetadata(string content){
            var metadata = new Dictionary<string, string>();
            if(!content.IsSet())
                return metadata;

            var startIndex = content.IndexOf(YAML_DEL_START);
            if(startIndex < 0)
                return metadata;

            startIndex += YAML_DEL_START.Length;

            var length = content.Substring(startIndex).IndexOf(YAML_DEL_END);
            if(length < 0)
                return metadata;

            var yamlString = content.Substring(startIndex, length);

            var deserializer = new DeserializerBuilder().Build();
            try{
                metadata = deserializer.Deserialize<Dictionary<string, string>>(yamlString);
            }
            catch(Exception e){
                this._log.Error<RazorProcessor>("Error parsing YAML metadata: " + e.Message);
            }

            return metadata;
        }

        private bool IsCshtmlFile(InputFile file){
            return file.Extension.IgnoreCaseEquals(".cshtml");
        }
    }
}