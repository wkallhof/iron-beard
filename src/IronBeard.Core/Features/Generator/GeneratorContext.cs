using System.Collections.Generic;
using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Features.Generator
{
    public class GeneratorContext
    {
        public InputFile Layout { get; set; }

        public string OutputDirectory { get; }
        public string InputDirectory { get; }

        public IEnumerable<InputFile> InputFiles { get; set; }
        public IEnumerable<OutputFile> OutputFiles { get; set; }

        public GeneratorContext(string inputDir, string outputDir){
            this.InputDirectory = inputDir;
            this.OutputDirectory = outputDir;
            this.InputFiles = new List<InputFile>();
            this.OutputFiles = new List<OutputFile>();
        }
    }
}