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
    public class RazorFileProcessor : IProcessor
    {
        private IFileSystem _fileSystem;
        private ILogger _log;
        private RazorViewRenderer _renderer;
        private const string YAML_DEL_START = "@*META";
        private const string YAML_DEL_END = "*@";

        public RazorFileProcessor(IFileSystem fileSystem, ILogger logger, string inputDirectory){
            this._fileSystem = fileSystem;
            this._log = logger;
            this._renderer = new RazorViewRenderer(inputDirectory);
        }

        public Task PreProcessAsync(InputFile file, GeneratorContext context)
        {
            // set the layout if it is found
            //TODO: Consider how to support multiple layouts
            if(this.IsCshtmlFile(file) && file.Name.IgnoreCaseEquals(Config.LayoutFileName))
                context.Layout = file;

            return Task.CompletedTask;
        }

        public async Task<OutputFile> ProcessAsync(InputFile file, GeneratorContext context)
        {
            // if this isn't CSHTML, or this is a Layout, or a partial, ignore
            if (!this.IsCshtmlFile(file) || context.Layout.Equals(file) || file.Name.StartsWith("_"))
                return null;

            this._log.Info($"[Razor] Processing Input : {file.RelativePath}");

            var fileContent = await this._fileSystem.ReadAllTextAsync(file.FullPath);
            if(!fileContent.IsSet())
                return null;

            var metadata = this.ExtractYamlMetadata(fileContent);

            var output = OutputFile.FromInputFile(file);
            output.Extension = ".html";
            output.BaseDirectory = context.OutputDirectory;
            output.Metadata = metadata;
            output.Url = UrlProvider.GetUrl(file);

            return output;
        }

        public async Task PostProcessAsync(OutputFile file, GeneratorContext context)
        {
            if(file.Input.Extension.IgnoreCaseEquals(".md"))
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
                this._log.Error("Error parsing YAML metadata: " + e.Message);
            }

            return metadata;
        }

        private bool IsCshtmlFile(InputFile file){
            return file.Extension.IgnoreCaseEquals(".cshtml");
        }
    }
}