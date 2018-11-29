using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Shared;
using Markdig;
using Markdig.Extensions.Yaml;
using YamlDotNet.Serialization;

namespace IronBeard.Core.Features.Markdown
{
    /// <summary>
    /// Processor responsible for converting Markdown .md files into HTML
    /// </summary>
    public class MarkdownProcessor : IProcessor
    {
        private IFileSystem _fileSystem;
        private IUrlProvider _urlProvider;
        private GeneratorContext _context;
        private BeardConfig _config;
        private ILogger _log;
        private MarkdownPipeline _pipeline = null;
        private const string YAML_DEL = "---";

        public MarkdownProcessor(IFileSystem fileSystem, ILogger logger, IUrlProvider urlProvider, BeardConfig config, GeneratorContext context){
            this._log = logger;
            this._fileSystem = fileSystem;
            this._urlProvider = urlProvider;
            this._context = context;
            this._config = config;

            // Enable MarkdownExtensions if configured
            if (_config.EnableMarkdownExtensions)
            {
                _pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Build();
            }
        }

        // no pre-processing required
        public Task PreProcessAsync(InputFile file) => Task.CompletedTask;

        /// <summary>
        /// Main markdown processing. Determines if the given file is a markdown file, and if so,
        /// pulls out metadata and converts content into HTML
        /// </summary>
        /// <param name="file">File to process</param>
        /// <returns>OutputFile if InputFile was markdown</returns>
        public async Task<OutputFile> ProcessAsync(InputFile file)
        {
            if (!file.Extension.ToLower().Equals(".md"))
                return null;

            this._log.Info<MarkdownProcessor>($"Processing Input: {file.RelativePath}");

            var markdown = await this._fileSystem.ReadAllTextAsync(file.FullPath);
            if (!markdown.IsSet())
                return null;

            // extract our metadata
            var result = this.ExtractYamlMetadata(markdown);

            // convert markdown to HTML
            var html = Markdig.Markdown.ToHtml(result.markdown, _pipeline);

            var output = OutputFile.FromInputFile(file);
            output.Content = html;
            output.Extension = ".html";
            output.BaseDirectory = this._context.OutputDirectory;
            output.Metadata = result.metadata;
            output.Url = this._urlProvider.GetUrl(file);

            return output;
        }

        /// <summary>
        /// Extracts YAML frontmatter from the beginning of the markdown file content.
        /// Converts this YAML to a dictionary for reference in Razor rendering
        /// </summary>
        /// <param name="markdown">Markdown content</param>
        /// <returns>Markdown content without YAML, Metadata dictionary</returns>
        private (string markdown, Dictionary<string,string> metadata) ExtractYamlMetadata(string markdown){
            var metadata = new Dictionary<string, string>();
            // ensure we have HTML and that it starts with our delimiter (---)
            if(!markdown.IsSet() || !markdown.StartsWith(YAML_DEL))
                return (markdown, metadata);

            // ensure we have a second delimiter (---)
            var endDelimiterIndex = markdown.Substring(YAML_DEL.Length-1).IndexOf(YAML_DEL);
            if(endDelimiterIndex < 0)
                return (markdown, metadata);

            // pull out YAML string
            var yamlString = markdown.Substring(YAML_DEL.Length, endDelimiterIndex-1);
            markdown = markdown.Substring(endDelimiterIndex + (YAML_DEL.Length*2));

            var deserializer = new DeserializerBuilder().Build();
            try{
                metadata = deserializer.Deserialize<Dictionary<string, string>>(yamlString);
            }
            catch(Exception e){
                this._log.Error<MarkdownProcessor>("Error parsing YAML metadata: " + e.Message);
            }

            return (markdown, metadata);
        }

        public Task PostProcessAsync(OutputFile file) => Task.CompletedTask;
    }
}