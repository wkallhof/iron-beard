using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Shared;

namespace IronBeard.Core.Features.Formatting
{
    public class HtmlFormatProcessor : IProcessor
    {
        private ILogger _log;

        public HtmlFormatProcessor(ILogger logger){
            this._log = logger;
        }

        public Task AfterProcessAsync(OutputFile file, GeneratorContext context)
        {
            if(!file.Extension.IgnoreCaseEquals(".html"))
                return Task.CompletedTask;

            this._log.Info("[HTML FORMATTER] Formatting " + file.RelativePath);
            try
            {
                file.Content = XElement.Parse(file.Content).ToString();
                return Task.CompletedTask;
            }
            catch(Exception e)
            {
                this._log.Warn($"[HTML FORMATTER] {file.RelativePath} isn't well formed XML / HTML : {e.Message}");
                return Task.CompletedTask;
            }
        }

        public Task BeforeProcessAsync(InputFile file, GeneratorContext context) => Task.CompletedTask;
        public Task<OutputFile> ProcessAsync(InputFile file, GeneratorContext context) => Task.FromResult<OutputFile>(null);
    }
}