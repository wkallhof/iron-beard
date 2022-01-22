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
        private readonly List<IProcessor> _processors;
        private readonly IFileSystem _fileSystem;
        private readonly GeneratorContext _context;
        private readonly ILogger _log;

        public StaticGenerator(IFileSystem fileSystem, ILogger logger, GeneratorContext context){
            _log = logger;
            _processors = new List<IProcessor>();
            _context = context;

            _fileSystem = fileSystem ?? throw new ArgumentException("File System not provided");

            if (!context.InputDirectory.IsSet())
                throw new ArgumentException("Input Directory not provided");

            if(!context.OutputDirectory.IsSet())
                throw new ArgumentException("Output Directory not provided");

            
        }

        /// <summary>
        /// Allows consuming application to define the processors used
        /// </summary>
        /// <param name="processor">Processor to add to pipeline</param>
        public void AddProcessor(IProcessor processor) => _processors.Add(processor);

        /// <summary>
        /// Starts the static generator process. Scans files, iterates through
        /// processors, and eventually writes files.
        /// </summary>
        /// <returns>Task</returns>
        public async Task Generate(){
            try{
                if(!_processors.Any())
                    throw new Exception("No processors added to generator.");

                _log.Info<StaticGenerator>("Starting IronBeard...");

                _log.Info<StaticGenerator>("Clearing output directory...");
                await _fileSystem.DeleteDirectoryAsync(_context.OutputDirectory);

                _log.Info<StaticGenerator>("Creating temp directory...");
                await _fileSystem.CreateTempFolderAsync(_context.InputDirectory);

                _log.Info<StaticGenerator>("Loading files...");
                _context.InputFiles = _fileSystem.GetFiles(_context.InputDirectory).ToList();

                _log.Info<StaticGenerator>("Pre-Processing...");
                await RunPreProcessing();

                _log.Info<StaticGenerator>("Processing...");
                await RunProcessing();

                _log.Info<StaticGenerator>("Post-Processing...");
                await RunPostProcessing();

                _log.Info<StaticGenerator>("Writing files...");
                await _fileSystem.WriteOutputFilesAsync(_context.OutputFiles);
            }
            finally
            {
                _log.Info<StaticGenerator>("Deleting temp directory...");
                await _fileSystem.DeleteTempFolderAsync();
            }
        }

        /// <summary>
        /// Pre-process our files. Useful for scanning input files, finding layouts, etc.
        /// </summary>
        /// <returns>Task</returns>
        private async Task RunPreProcessing(){
            foreach(var processor in _processors)
                foreach(var file in _context.InputFiles)
                    await processor.PreProcessAsync(file);
        }

        /// <summary>
        /// Main processing pass. Generates OutputFiles from our InputFiles
        /// for further processing
        /// </summary>
        /// <returns>Task</returns>
        private async Task RunProcessing(){
            var outputFiles = new Dictionary<string, OutputFile>();
            foreach(var processor in _processors){
                foreach(var file in _context.InputFiles){
                    var output = await processor.ProcessAsync(file);
                    if(output == null)
                        continue;

                    if(!outputFiles.ContainsKey(output.FullPath))
                        outputFiles.Add(output.FullPath, output);
                }
            }

            _context.OutputFiles = outputFiles.Select(x => x.Value);
        }

        /// <summary>
        /// Post-processing of our OutputFiles. This is the last step
        /// of processing before files are written to disk
        /// </summary>
        /// <returns></returns>
        private async Task RunPostProcessing(){
            foreach(var processor in _processors)
                foreach(var file in _context.OutputFiles)
                    await processor.PostProcessAsync(file);
        }        
    }
}