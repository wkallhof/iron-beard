using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Shared;
using Microsoft.Extensions.Options;

namespace IronBeard.Core.Features.Static
{
    /// <summary>
    /// Static processor responsible for processing our static files.
    /// In its current implementation, it simply copies the static files
    /// to their output directory
    /// </summary>
    public class StaticProcessor : IProcessor
    {
        private List<string> _ignoreExtensions;
        private ILogger _log;
        private IUrlProvider _urlProvider;
        private BeardConfig _config;
        private GeneratorContext _context;

        public StaticProcessor(ILogger logger, IUrlProvider urlProvider, BeardConfig config, GeneratorContext context){
            this._log = logger;
            this._urlProvider = urlProvider;
            this._config = config;
            this._context = context;
            this._ignoreExtensions = this._config.StaticExtensionIgnoreList.Select(x => x.ToLower()).ToList();
        }
        
        /// <summary>
        /// Main process action to define the destination file paths
        /// </summary>
        /// <param name="file">Intput file</param>
        /// <returns>OutputFile if static file</returns>
        public Task<OutputFile> ProcessAsync(InputFile file)
        {
            // If our file is something to ignore, ignore it
            if(this._ignoreExtensions.Contains(file.Extension.ToLower()))
                return Task.FromResult<OutputFile>(null);

            this._log.Info<StaticProcessor>($"Processing Input: {file.RelativePath}");

            // create OutputFile with the Direct Copy flag set to true
            var output = OutputFile.FromInputFile(file);
            output.DirectCopy = true;
            output.BaseDirectory = this._context.OutputDirectory;
            output.Url = this._urlProvider.GetUrl(file);

            return Task.FromResult(output);
        }

        public Task PreProcessAsync(InputFile file) => Task.CompletedTask;
        public Task PostProcessAsync(OutputFile file) => Task.CompletedTask;
    }
}