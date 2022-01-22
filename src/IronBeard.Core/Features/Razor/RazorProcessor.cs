using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Shared;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Logging;
using YamlDotNet.Serialization;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Configuration;

namespace IronBeard.Core.Features.Razor
{
    /// <summary>
    /// The beef of the static generator, this is responsible for processing razor files,
    /// wrapping both Markdown and Razor files in a consistent layout, etc.
    /// </summary>
    public class RazorProcessor : IProcessor
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _log;
        private readonly IUrlProvider _urlProvider;
        private readonly BeardConfig _config;
        private readonly RazorViewRenderer _renderer;
        private readonly GeneratorContext _context;

        private const string YAML_DEL_START = "@*META";
        private const string YAML_DEL_END = "*@";

        public RazorProcessor(IFileSystem fileSystem, RazorViewRenderer renderer, 
            ILogger logger, IUrlProvider urlProvider, BeardConfig config, GeneratorContext context){
            _fileSystem = fileSystem;
            _log = logger;
            _urlProvider = urlProvider;
            _config = config;
            _renderer = renderer;
            _context = context;
        }

        /// <summary>
        /// Scans the input files for any layout files. Sets context
        /// accordingly if found
        /// </summary>
        /// <param name="file">File to pre-process</param>
        /// <returns>Task</returns>
        public Task PreProcessAsync(InputFile file)
        {
            if(IsCshtmlFile(file) && file.Name.IgnoreCaseEquals(_config.LayoutFileName))
                _context.Layout = file;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Extracts YAML metadata from the given file if it is a Razor file
        /// </summary>
        /// <param name="file">File to process</param>
        /// <returns>OutputFile if InputFile was Razor</returns>
        public async Task<OutputFile?> ProcessAsync(InputFile file)
        {
            // if this isn't CSHTML, or this is a Layout, or a partial, ignore
            if (!IsCshtmlFile(file) || (_context.Layout != null && _context.Layout.Equals(file)) || file.Name.StartsWith("_"))
                return null;

            _log.Info<RazorProcessor>($"Processing Input : {file.RelativePath}");

            var fileContent = await _fileSystem.ReadAllTextAsync(file.FullPath);
            if(!fileContent.IsSet())
                return null;

            var metadata = ExtractYamlMetadata(fileContent);

            var output = new OutputFile(file, _context.OutputDirectory)
            {
                Extension = ".html",
                Metadata = metadata,
                Url = _urlProvider.GetUrl(file)
            };

            return output;
        }

        /// <summary>
        /// Main Razor Processing, done as a post-processor once all other processors have
        /// written their content.
        /// </summary>
        /// <param name="file">File to post-process</param>
        /// <returns>Task</returns>
        public async Task PostProcessAsync(OutputFile file)
        {
            // if the file was a Markdown file, wrap the content in layout
            if(file.Input.Extension.IgnoreCaseEquals(".md"))
                await ProcessMarkdown(file);

            // if the file was a razor file (cshtml), wrap in layout and render to HTML
            if(IsCshtmlFile(file.Input) && _context.Layout != file.Input && !file.Input.Name.StartsWith("_"))
                await ProcessRazor(file);
        }

        /// <summary>
        /// Processes the given file as a Markdown file and wraps it in
        /// the layout markup if it exists
        /// </summary>
        /// <param name="file">Markdown file</param>
        /// <returns>Task</returns>
        private async Task ProcessMarkdown(OutputFile file){
            _log.Info<RazorProcessor>($"Processing Markdown Output : { file.RelativePath }");

            // build ViewContext for passing into renderer
            var viewContext = new ViewContext(file, _context, _config);

            // we already have the HTML from the markdown since it happened in the process stage. Pass to render
            file.Content = await CreateTempAndRender(file.Content, file.FullPath, viewContext);
        }

        /// <summary>
        /// Processes the given file as a Razor file, wrapping it in the layout
        /// if one exists
        /// </summary>
        /// <param name="file">Razor file</param>
        /// <returns>Task</returns>
        private async Task ProcessRazor(OutputFile file){
            _log.Info<RazorProcessor>($"Processing Razor Output : { file.Input.RelativePath }");

            // build ViewContext for passing into renderer
            var viewContext = new ViewContext(file, _context, _config);

            // we haven't processed the file content yet. Read it in for processing
            var fileContent = await _fileSystem.ReadAllTextAsync(file.Input.FullPath);
            if(!fileContent.IsSet())
                return;

            file.Content = await CreateTempAndRender(fileContent, file.Input.FullPath, viewContext);
        }

        /// <summary>
        /// Given the file content, reference, and current ViewContext,
        /// render the content as Razor template
        /// </summary>
        /// <param name="fileContent">File content</param>
        /// <param name="fullFilePath">Full file path to file being processed</param>
        /// <param name="viewContext">Current view context</param>
        /// <returns>Rendered file content</returns>
        private async Task<string> CreateTempAndRender(string? fileContent, string fullFilePath, ViewContext viewContext){

            // append our Layout info to the top of the file if we have a layout defined
            var html = AppendLayoutInfo(fileContent, _context.Layout);

            // create a temp file with the view content. The RazorViewRenderer requires a file on
            //disk to render, so we need to create one temporarily
            var tempFile = await _fileSystem.CreateTempFileAsync(html, ".cshtml");
            try{
                // render the razor template with the given view context
                return await _renderer.RenderAsync(tempFile.RelativePath, viewContext);
            }
            catch(Exception e)
            {
                // there was a problem, likely a razor formatting issues or issue with our
                // renderer not finding the view file. In the event the issue with a problem with our
                // razor syntax, make sure we replace the temp file path with the original path so the user
                // knows where to find the syntax error.
                var message = $"{fullFilePath}: {e.Message.Replace(tempFile.FullPath, fullFilePath)}";
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Responsible for appending the layout reference content to the top of
        /// the given file content if we have a layout defined in our context.
        /// </summary>
        /// <param name="fileContent">File to append layout info to</param>
        /// <param name="layout">Layout reference</param>
        /// <returns>Content with layout path</returns>
        private static string AppendLayoutInfo(string? fileContent, InputFile? layout){
            if(layout == null)
                return string.Empty;

            var relativePath = Path.Combine("~", layout.RelativePath);

            // for windows support, we need to flip path delimeters
            relativePath = relativePath.Replace("\\","/");

            // add the expected layout definition to the top of the file ex. @{ Layout = ~/path/to/_Layout.cshtml; }
            var razorLayoutString = $"@{{ Layout = \"{ relativePath }\"; }}\n";
            return razorLayoutString + fileContent;
        }

        /// <summary>
        /// Responsible for extracting any metadata found in the Razor file.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private Dictionary<string,string> ExtractYamlMetadata(string content){
            var metadata = new Dictionary<string, string>();
            if(!content.IsSet())
                return metadata;

            var startIndex = content.IndexOf(YAML_DEL_START);
            if(startIndex < 0)
                return metadata;

            startIndex += YAML_DEL_START.Length;

            var length = content[startIndex..].IndexOf(YAML_DEL_END);
            if(length < 0)
                return metadata;

            // parsed out YAML frontmatter string
            var yamlString = content.Substring(startIndex, length);

            var deserializer = new DeserializerBuilder().Build();
            try{
                metadata = deserializer.Deserialize<Dictionary<string, string>>(yamlString);
            }
            catch(Exception e){
                _log.Error<RazorProcessor>("Error parsing YAML metadata: " + e.Message);
            }

            return metadata;
        }

        /// <summary>
        /// Determine if the file is a CSHTML File or not
        /// </summary>
        /// <param name="file">File to determine if CSHTML</param>
        /// <returns>True of CSHTML, False if not</returns>
        private static bool IsCshtmlFile(InputFile file) 
            => file.Extension.IgnoreCaseEquals(".cshtml");
    }
}