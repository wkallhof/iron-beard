using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Raud.Core.Features.FileSystem;
using Raud.Core.Features.Shared;

namespace Raud.Core.Features.Static
{
    public class StaticFileProcessor : IFileProcessor
    {
        private List<string> _ignoreExtensions;

        public StaticFileProcessor(List<string> ignore){
            this._ignoreExtensions = ignore.Select(x => x.ToLower()).ToList();
        }

        public Task<(bool processed, OutputFile file)> ProcessInputAsync(InputFile file, string outputDirectory)
        {
            if(this._ignoreExtensions.Contains(file.Extension.ToLower()))
                return Task.FromResult<(bool processed, OutputFile file)>((false, null));
            
            Console.WriteLine($"[Static] Processing Input: {Path.Combine(file.RelativeDirectory, file.Name + file.Extension)}");
            var output = OutputFile.FromInputFile(file);
            output.DirectCopy = true;
            output.FullDirectory = Path.GetFullPath(outputDirectory + output.RelativeDirectory);
            output.FullPath = Path.Combine(output.FullDirectory, output.Name + output.Extension);

            return Task.FromResult<(bool processed, OutputFile file)>((true, output));
        }

        public Task ProcessOutputAsync(OutputFile file) => Task.CompletedTask;
    }
}