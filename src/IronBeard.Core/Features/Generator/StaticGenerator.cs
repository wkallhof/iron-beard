using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Shared;

namespace IronBeard.Core.Features.Generator
{
    public class StaticGenerator
    {
        private List<IProcessor> _processors;
        private IFileSystem _fileSystem;
        private GeneratorContext _context;

        public StaticGenerator(IFileSystem fileSystem, string inputDir, string outputDir){
            this._processors = new List<IProcessor>();

            if(fileSystem == null)
                throw new ArgumentException("File System not provided");

            if(!inputDir.IsSet())
                throw new ArgumentException("Input Directory not provided");

            if(!outputDir.IsSet())
                throw new ArgumentException("Output Directory not provided");

            this._fileSystem = fileSystem;
            this._context = new GeneratorContext(inputDir, outputDir);
        }

        public void AddProcessor(IProcessor processor) => this._processors.Add(processor);

        public async Task Generate(){
            try{
                if(!this._processors.Any())
                    throw new Exception("No processors added to generator.");

                Console.WriteLine("Clearing output directory...");
                await this._fileSystem.DeleteDirectoryAsync(this._context.OutputDirectory);

                Console.WriteLine("Creating temp directory...");
                await this._fileSystem.CreateTempFolderAsync(this._context.InputDirectory);

                Console.WriteLine("Loading files...");
                this._context.InputFiles = this._fileSystem.GetFiles(this._context.InputDirectory).ToList();

                Console.WriteLine("Running Before Process...");
                await this.RunBeforeProcess();

                Console.WriteLine("Running Process...");
                await this.RunProcess();

                Console.WriteLine("Running After Process...");
                await this.RunAfterProcess();

                Console.WriteLine("Writing files...");
                await this._fileSystem.WriteOutputFilesAsync(this._context.OutputFiles);
            }
            finally
            {
                Console.WriteLine("Deleting temp directory...");
                await this._fileSystem.DeleteTempFolderAsync();
            }

        }

        private async Task RunBeforeProcess(){
            foreach(var processor in this._processors)
                foreach(var file in this._context.InputFiles)
                    await processor.BeforeProcessAsync(file, this._context);
        }

        private async Task RunProcess(){
            var outputFiles = new Dictionary<string, OutputFile>();
            foreach(var processor in this._processors){
                foreach(var file in this._context.InputFiles){
                    var output = await processor.ProcessAsync(file, this._context);
                    if(output == null)
                        continue;

                    if(!outputFiles.ContainsKey(output.FullPath))
                        outputFiles.Add(output.FullPath, output);
                }
            }

            this._context.OutputFiles = outputFiles.Select(x => x.Value);
        }

        private async Task RunAfterProcess(){
            foreach(var processor in this._processors)
                foreach(var file in this._context.OutputFiles)
                    await processor.AfterProcessAsync(file, this._context);
        }
    }
}