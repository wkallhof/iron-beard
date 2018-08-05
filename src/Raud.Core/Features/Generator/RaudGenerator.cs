using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Raud.Core.Extensions;
using Raud.Core.Features.FileSystem;
using Raud.Core.Features.Shared;

namespace Raud.Core.Features.Generator
{
    public class RaudGenerator
    {
        private List<IFileProcessor> _fileProcessors;
        private IFileSystem _fileSystem;
        private string _inputDirectory;
        private string _outputDirectory;

        public RaudGenerator(IFileSystem fileSystem, string inputDir, string outputDir){
            this._fileProcessors = new List<IFileProcessor>();

            if(fileSystem == null)
                throw new ArgumentException("File System not provided");

            if(!inputDir.IsSet())
                throw new ArgumentException("Input Directory not provided");

            if(!outputDir.IsSet())
                throw new ArgumentException("Output Directory not provided");

            this._inputDirectory = inputDir;
            this._outputDirectory = outputDir;
            this._fileSystem = fileSystem;
        }

        public void AddProcessor(IFileProcessor processor) => this._fileProcessors.Add(processor);

        public async Task Generate(){
            if(!this._fileProcessors.Any())
                throw new Exception("No processors added to generator.");

            Console.WriteLine("Clearing output directory...");
            await this._fileSystem.DeleteDirectoryAsync(this._outputDirectory);

            Console.WriteLine("Creating temp directory...");
            await this._fileSystem.CreateTempFolderAsync(this._inputDirectory);

            Console.WriteLine("Loading files...");
            var inputs = this._fileSystem.GetFiles(this._inputDirectory);

            Console.WriteLine("Processing Inputs...");
            var outputFiles = await this.ProcessInputs(inputs, this._outputDirectory);

            Console.WriteLine("Processing Outputs...");
            outputFiles = await this.ProcessOutputs(outputFiles);

            Console.WriteLine("Deleting temp directory...");
            await this._fileSystem.DeleteTempFolderAsync();

            Console.WriteLine("Writing files...");
            await this._fileSystem.WriteOutputFilesAsync(outputFiles);
        }

        private async Task<IEnumerable<OutputFile>> ProcessInputs(IEnumerable<InputFile> files, string outputDir){
            var outputFiles = new Dictionary<string, OutputFile>();
            foreach(var processor in this._fileProcessors){
                foreach(var file in files){
                    var (processed, output) = await processor.ProcessInputAsync(file, outputDir);
                    if(!processed || output == null)
                        continue;

                    if(!outputFiles.ContainsKey(output.FullPath))
                        outputFiles.Add(output.FullPath, output);
                }
            }

            return outputFiles.Select(x => x.Value);
        }

        private async Task<IEnumerable<OutputFile>> ProcessOutputs(IEnumerable<OutputFile> files){
            foreach(var processor in this._fileProcessors){
                foreach(var file in files){
                    await processor.ProcessOutputAsync(file);
                }
            }

            return files;
        }
    }
}