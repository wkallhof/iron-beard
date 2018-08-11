using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Shared;

namespace IronBeard.Core.Features.Static
{
    public class StaticFileProcessor : IProcessor
    {
        private List<string> _ignoreExtensions;

        public StaticFileProcessor(List<string> ignore){
            this._ignoreExtensions = ignore.Select(x => x.ToLower()).ToList();
        }

        public Task BeforeProcessAsync(InputFile file, GeneratorContext context) => Task.CompletedTask;

        public Task<OutputFile> ProcessAsync(InputFile file, GeneratorContext context)
        {
            if(this._ignoreExtensions.Contains(file.Extension.ToLower()))
                return Task.FromResult<OutputFile>(null);

            //Console.WriteLine($"[Static] Processing Input: {file.RelativePath}");
            var output = OutputFile.FromInputFile(file);
            output.DirectCopy = true;
            output.BaseDirectory = context.OutputDirectory;
            return Task.FromResult(output);
        }

        public Task AfterProcessAsync(OutputFile file, GeneratorContext context) => Task.CompletedTask;
    }
}