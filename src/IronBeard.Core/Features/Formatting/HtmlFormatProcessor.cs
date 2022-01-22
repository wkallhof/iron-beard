using System.Xml.Linq;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Shared;

namespace IronBeard.Core.Features.Formatting
{
    /// <summary>
    /// A processor whose sole task is to format outgoing HTML.
    /// 
    /// Razor tends to create sloppy HTML output with hard to follow markup indenting.
    /// This will ensure that the rendered HTML is easy to read. It also doubles as a HTML syntax
    /// validator as it can't format improperly defined markup (missing closing tags, etc.)
    /// </summary>
    public class HtmlFormatProcessor : IProcessor
    {
        private readonly ILogger _log;

        public HtmlFormatProcessor(ILogger logger){
            _log = logger;
        }

        /// <summary>
        /// We only need to handle the Post Processing of files for this one.
        /// It scans outgoing .HTML files and cleans up the content by formatting with
        /// the XML formatter.
        /// </summary>
        /// <param name="file">File to process</param>
        /// <returns>Task</returns>
        public Task PostProcessAsync(OutputFile file)
        {
            // If it isn't an HTML or it doesn't have content, don't bother
            if(!file.Extension.IgnoreCaseEquals(".html") || !file.Content.IsSet())
                return Task.CompletedTask;

            _log.Info<HtmlFormatProcessor>("Formatting " + file.RelativePath);
            try
            {
                // simply by parsing with XElement, it formats the content
                file.Content = XElement.Parse(file.Content!).ToString();
            }
            catch(Exception e)
            {
                // The only real exception thrown relates to the content not being well formed
                _log.Warn<HtmlFormatProcessor>($"{file.RelativePath} isn't well formed XML / HTML : {e.Message}");
            }
            return Task.CompletedTask;
        }

        public Task PreProcessAsync(InputFile file) => Task.CompletedTask;
        public Task<OutputFile?> ProcessAsync(InputFile file) => Task.FromResult<OutputFile?>(null);
    }
}