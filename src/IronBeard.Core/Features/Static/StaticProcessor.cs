using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Shared;

namespace IronBeard.Core.Features.Static
{
    /// <summary>
    /// Static processor responsible for processing static files like images.
    /// In its current implementation, it simply copies the static files
    /// to their output directory
    /// </summary>
    public class StaticProcessor : IProcessor
    {
        private readonly List<string> _ignoreExtensions;
        private readonly ILogger _log;
        private readonly IUrlProvider _urlProvider;
        private readonly BeardConfig _config;
        private readonly GeneratorContext _context;

        public StaticProcessor(ILogger logger, IUrlProvider urlProvider, BeardConfig config, GeneratorContext context){
            _log = logger;
            _urlProvider = urlProvider;
            _config = config;
            _context = context;
            _ignoreExtensions = _config.StaticExtensionIgnoreList.Select(x => x.ToLower()).ToList();
        }
        
        /// <summary>
        /// Main process action to define the destination file paths
        /// </summary>
        /// <param name="file">Intput file</param>
        /// <returns>OutputFile if static file</returns>    
        public Task<OutputFile?> ProcessAsync(InputFile file)
        {
            // If our file is something to ignore, ignore it
            if(_ignoreExtensions.Contains(file.Extension.ToLower()))
                return Task.FromResult<OutputFile?>(null);

            _log.Info<StaticProcessor>($"Processing Input: {file.RelativePath}");

            // create OutputFile with the Direct Copy flag set to true
            var output = new OutputFile(file, _context.OutputDirectory)
            {
                DirectCopy = true,
                Url = _urlProvider.GetUrl(file)
            };

            return Task.FromResult<OutputFile?>(output);
        }

        public Task PreProcessAsync(InputFile file) => Task.CompletedTask;
        public Task PostProcessAsync(OutputFile file) => Task.CompletedTask;
    }
}