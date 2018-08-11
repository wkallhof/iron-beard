using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Shared;

namespace IronBeard.Core.Features.Static
{
    public class StaticFileProcessor : IProcessor
    {
        private List<string> _ignoreExtensions;
        private ILogger _log;

        public StaticFileProcessor(ILogger logger, List<string> ignore){
            this._log = logger;
            this._ignoreExtensions = ignore.Select(x => x.ToLower()).ToList();
        }

        public Task PreProcessAsync(InputFile file, GeneratorContext context) => Task.CompletedTask;

        public Task<OutputFile> ProcessAsync(InputFile file, GeneratorContext context)
        {
            if(this._ignoreExtensions.Contains(file.Extension.ToLower()))
                return Task.FromResult<OutputFile>(null);

            this._log.Info($"[Static] Processing Input: {file.RelativePath}");

            var output = OutputFile.FromInputFile(file);
            output.DirectCopy = true;
            output.BaseDirectory = context.OutputDirectory;
            output.Url = UrlProvider.GetUrlWithExtension(file);

            return Task.FromResult(output);
        }

        public Task PostProcessAsync(OutputFile file, GeneratorContext context) => Task.CompletedTask;
    }
}