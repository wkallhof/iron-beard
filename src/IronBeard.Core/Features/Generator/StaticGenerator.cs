using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Shared;

namespace IronBeard.Core.Features.Generator
{
    /// <summary>
    /// Main static generator.
    /// </summary>
    public class StaticGenerator
    {
        private List<IProcessor> _processors;
        private IFileSystem _fileSystem;
        private GeneratorContext _context;
        private ILogger _log;

        public StaticGenerator(IFileSystem fileSystem, ILogger logger, GeneratorContext context){
            this._log = logger;
            this._processors = new List<IProcessor>();
            this._context = context;

            if(fileSystem == null)
                throw new ArgumentException("File System not provided");

            if(!context.InputDirectory.IsSet())
                throw new ArgumentException("Input Directory not provided");

            if(!context.OutputDirectory.IsSet())
                throw new ArgumentException("Output Directory not provided");

            this._fileSystem = fileSystem;
        }

        /// <summary>
        /// Allows consuming application to define the processors used
        /// </summary>
        /// <param name="processor">Processor to add to pipeline</param>
        public void AddProcessor(IProcessor processor) => this._processors.Add(processor);

        /// <summary>
        /// Starts the static generator process. Scans files, iterates through
        /// processors, and eventually writes files.
        /// </summary>
        /// <returns>Task</returns>
        public async Task Generate(){
            bool errorOccured = false;
            try{
                if(!this._processors.Any())
                    throw new Exception("No processors added to generator.");

                this._log.Info<StaticGenerator>("Starting IronBeard...");

                this._log.Info<StaticGenerator>("Clearing output directory...");
                await this._fileSystem.DeleteDirectoryAsync(this._context.OutputDirectory);

                this._log.Info<StaticGenerator>("Creating temp directory...");
                await this._fileSystem.CreateTempFolderAsync(this._context.InputDirectory);

                this._log.Info<StaticGenerator>("Loading files...");
                this._context.InputFiles = this._fileSystem.GetFiles(this._context.InputDirectory).ToList();

                this._log.Info<StaticGenerator>("Pre-Processing...");
                await this.RunPreProcessing();

                this._log.Info<StaticGenerator>("Processing...");
                await this.RunProcessing();

                this._log.Info<StaticGenerator>("Post-Processing...");
                await this.RunPostProcessing();

                this._log.Info<StaticGenerator>("Writing files...");
                await this._fileSystem.WriteOutputFilesAsync(this._context.OutputFiles);
            }
            catch (Exception ex)
            {
                //We don't want to actually catch the exception, just set a flag if it occurs.
                errorOccured = true;
                throw ex;
            }
            finally
            {
                if (errorOccured && _context.LeaveTempDirOnError)
                {
                    this._log.Error<StaticGenerator>("An error has occured. Leaving the temporary directory intact.\nThe folder will have to be deleted manually.");
                } else
                {
                    this._log.Info<StaticGenerator>("Deleting temp directory...");
                    await this._fileSystem.DeleteTempFolderAsync();
                }
            }
        }

        /// <summary>
        /// Pre-process our files. Useful for scanning input files, finding layouts, etc.
        /// </summary>
        /// <returns>Task</returns>
        private async Task RunPreProcessing(){
            foreach(var processor in this._processors)
                foreach(var file in this._context.InputFiles)
                    await processor.PreProcessAsync(file);
        }

        /// <summary>
        /// Main processing pass. Generates OutputFiles from our InputFiles
        /// for further processing
        /// </summary>
        /// <returns>Task</returns>
        private async Task RunProcessing(){
            var outputFiles = new Dictionary<string, OutputFile>();
            foreach(var processor in this._processors){
                foreach(var file in this._context.InputFiles){
                    var output = await processor.ProcessAsync(file);
                    if(output == null)
                        continue;

                    if(!outputFiles.ContainsKey(output.FullPath))
                        outputFiles.Add(output.FullPath, output);
                }
            }

            this._context.OutputFiles = outputFiles.Select(x => x.Value);
        }

        /// <summary>
        /// Post-processing of our OutputFiles. This is the last step
        /// of processing before files are written to disk
        /// </summary>
        /// <returns></returns>
        private async Task RunPostProcessing(){
            foreach(var processor in this._processors)
                foreach(var file in this._context.OutputFiles)
                    await processor.PostProcessAsync(file);
        }        
    }
}