using System.Collections.Generic;
using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Features.Generator
{
    /// <summary>
    /// This represents the running context or state for the static generator.
    /// As progress is made through the different processor pipelines, this is continually updated.
    /// It also contains important context on working directories, layout files, etc.
    /// </summary>
    public class GeneratorContext
    {
        /// <summary>
        /// This Layout references any files that match the layout file name. This will be used
        /// by the Razor processor to wrap content in a layout.
        /// </summary>
        public InputFile? Layout { get; set; }

        /// <summary>
        /// The current working directory to write files to
        /// </summary>
        public string OutputDirectory { get; }

        /// <summary>
        /// The directory to scan files from
        /// </summary>
        public string InputDirectory { get; }

        /// <summary>
        /// All InputFiles scanned from InputDirectory
        /// </summary>
        public IEnumerable<InputFile> InputFiles { get; set; }

        /// <summary>
        /// Current collection of OutputFiles being operated on
        /// </summary>
        public IEnumerable<OutputFile> OutputFiles { get; set; }

        public GeneratorContext(string inputDir, string outputDir){
            InputDirectory = inputDir;
            OutputDirectory = outputDir;
            InputFiles = new List<InputFile>();
            OutputFiles = new List<OutputFile>();
        }
    }
}