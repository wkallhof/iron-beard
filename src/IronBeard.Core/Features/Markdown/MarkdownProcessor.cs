using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Shared;
using Markdig;
using Markdig.Extensions.Yaml;
using YamlDotNet.Serialization;

namespace IronBeard.Core.Features.Markdown
{
    public class MarkdownFileProcessor : IProcessor
    {
        private IFileSystem _fileSystem;
        private const string YAML_DEL = "---";

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

            var result = this.ExtractYamlMetadata(markdown);

            var html = Markdig.Markdown.ToHtml(result.markdown);

            var output = OutputFile.FromInputFile(file);
            output.Content = html;
            output.Extension = ".html";
            output.BaseDirectory = context.OutputDirectory;
            output.Metadata = result.metadata;

            return output;
        }

        private (string markdown, Dictionary<string,string> metadata) ExtractYamlMetadata(string markdown){
            var metadata = new Dictionary<string, string>();
            // ensure we have HTML and that it starts with our delimiter (---)
            if(!markdown.IsSet() || !markdown.StartsWith(YAML_DEL))
                return (markdown, metadata);

            // ensure we have a second delimiter (---)
            var endDelimiterIndex = markdown.Substring(YAML_DEL.Length-1).IndexOf(YAML_DEL);
            if(endDelimiterIndex < 0)
                return (markdown, metadata);


            var yamlString = markdown.Substring(YAML_DEL.Length, endDelimiterIndex-1);
            markdown = markdown.Substring(endDelimiterIndex + (YAML_DEL.Length*2));

            var deserializer = new DeserializerBuilder().Build();
            try{
                metadata = deserializer.Deserialize<Dictionary<string, string>>(yamlString);
            }
            catch(Exception e){
                Console.WriteLine("Error parsing YAML metadata: " + e.Message);
            }

            return (markdown, metadata);
        }

        public Task AfterProcessAsync(OutputFile file, GeneratorContext context) => Task.CompletedTask;
    }
}