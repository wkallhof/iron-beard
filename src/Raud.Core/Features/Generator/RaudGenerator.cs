using System;
using System.Collections.Generic;
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

            Console.WriteLine("Clearing output directory");
            await this._fileSystem.DeleteDirectoryAsync(this._outputDirectory);

            var inputs = this._fileSystem.GetFiles(this._inputDirectory);

            var outputFiles = await this.ProcessInputs(inputs, this._outputDirectory);
            outputFiles = await this.ProcessOutputs(outputFiles);

            await this._fileSystem.WriteOutputFilesAsync(outputFiles);
        }

        private async Task<IEnumerable<OutputFile>> ProcessInputs(IEnumerable<InputFile> files, string outputDir){
            var outputFiles = new List<OutputFile>();
            foreach(var processor in this._fileProcessors){
                foreach(var file in files){
                    Console.WriteLine("Processing Input File : " + file.FullPath);
                    var (processed, output) = await processor.ProcessInputAsync(file, outputDir);
                    if(!processed || output == null)
                        continue;

                    outputFiles.Add(output);
                }
            }

            // Grab unique values by path. TODO: Throw error if file conflict. 
            return outputFiles.GroupBy(x => x.FullPath).Select(x => x.First());
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