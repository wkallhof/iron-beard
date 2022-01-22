using System.Text;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Configuration;

namespace IronBeard.Core.Features.FileSystem
{
    /// <summary>
    /// Represents actions needed from the file system
    /// </summary>
    public interface IFileSystem
    {
        /// <summary>
        /// Get all files from the given directory path. This includes
        /// subdirectories.
        /// </summary>
        /// <param name="directoryPath">Directory path to scan files within</param>
        /// <returns>Collection of InputFiles</returns>
        IEnumerable<InputFile> GetFiles(string directoryPath);

        /// <summary>
        /// Read the text content of file at the given path.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>File content</returns>
        Task<string> ReadAllTextAsync(string filePath);

        /// <summary>
        /// Persist the given OutputFiles
        /// </summary>
        /// <param name="files">Files to persist</param>
        /// <returns>Task</returns>
        Task WriteOutputFilesAsync(IEnumerable<OutputFile> files);

        /// <summary>
        /// Persist the given OutputFile
        /// </summary>
        /// <param name="file">File to persist</param>
        /// <returns>Task</returns>
        Task WriteOutputFileAsync(OutputFile file);

        /// <summary>
        /// Delete the directory with the given directory path
        /// </summary>
        /// <param name="directoryPath">Directory path</param>
        /// <returns>Task</returns>
        Task DeleteDirectoryAsync(string directoryPath);

        /// <summary>
        /// Copies the given OutputFile's InputFile to the path
        /// defined on the Output
        /// </summary>
        /// <param name="file">File to copy</param>
        /// <returns>Task</returns>
        Task CopyOutputFileAsync(OutputFile file);

        /// <summary>
        /// Creates a temp folder in the given directory path
        /// </summary>
        /// <param name="directoryPath">Directory path to create temp folder in</param>
        /// <returns>New temp folder path</returns>
        Task<string> CreateTempFolderAsync(string directoryPath);

        /// <summary>
        /// Deletes any existing temp folders that were created
        /// </summary>
        /// <returns>Task</returns>
        Task DeleteTempFolderAsync();

        /// <summary>
        /// Creates a temp file in the created temp folder with the given
        /// file content and extension.
        /// </summary>
        /// <param name="content">Content to write in temp file</param>
        /// <param name="extension">Extension to use for temp file</param>
        /// <returns>InputFile reference to new file</returns>
        Task<InputFile> CreateTempFileAsync(string content, string extension);

    }

    /// <summary>
    /// IFileSystem implementation that utilizes actual 
    /// disk IO for operations
    /// </summary>
    public class DiskFileSystem : IFileSystem
    {
        private string? _tempFolderPath;
        private string? _tempFolderBase;
        private readonly ILogger _log;
        private readonly BeardConfig _config;

        public DiskFileSystem(ILogger logger, BeardConfig config){
            _log = logger;
            _config = config;
        }

        /// <summary>
        /// Get's all the files in the given directory path
        /// by enumerating them and mapping to InputFiles
        /// </summary>
        /// <param name="directoryPath">Directory to read files in</param>
        /// <returns>Collection of created InputFiles</returns>
        public IEnumerable<InputFile> GetFiles(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            return directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Select(x => MapFileInfoToInputFile(x, directoryPath));
        }

        /// <summary>
        /// Reads all text from the file at the given file path
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>File content</returns>
        public async Task<string> ReadAllTextAsync(string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Given a collection of OutputFiles, write each one to disk
        /// </summary>
        /// <param name="files">OutputFiles</param>
        /// <returns>Task</returns>
        public async Task WriteOutputFilesAsync(IEnumerable<OutputFile> files){
            foreach(var file in files){
                await WriteOutputFileAsync(file);
            }
        }

        /// <summary>
        /// Given an OutputFile, write it to disk
        /// </summary>
        /// <param name="file">File to write to disk</param>
        /// <returns>Task</returns>
        public async Task WriteOutputFileAsync(OutputFile file){

            // ensure we account for excluding HTML extensions
            if(_config.ExcludeHtmlExtension && file.Extension.IgnoreCaseEquals(".html") && !file.Name.Equals(_config.IndexFileName))
                file = file with { Extension = string.Empty };

            // if we are just supposed to copy the file, just copy it
            if(file.DirectCopy){
                await CopyOutputFileAsync(file);
                return;
            }
            
            // create our directory if we need to and write file to disk
            Directory.CreateDirectory(file.FullDirectory);
            using var writer = File.CreateText(file.FullPath);

            _log.Info<DiskFileSystem>($"Writing Output File : {Path.Combine(file.RelativeDirectory, file.Name + file.Extension)}");
            await writer.WriteLineAsync(file.Content).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the directory at the given path
        /// </summary>
        /// <param name="directoryPath">Path of directory to delete</param>
        /// <returns>Task</returns>
        public Task DeleteDirectoryAsync(string directoryPath){
            if(Directory.Exists(directoryPath))
                Directory.Delete(directoryPath, recursive: true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Copies the given OutputFile from its InputFile source
        /// to it's OutputFile target directory
        /// </summary>
        /// <param name="file">OutputFile to copy</param>
        /// <returns>Task</returns>
        public async Task CopyOutputFileAsync(OutputFile file){
            // ensure our target directory exists
            Directory.CreateDirectory(file.FullDirectory);
            using var source = File.Open(file.Input.FullPath, FileMode.Open);
            using var destination = File.Create(file.FullPath);

            await source.CopyToAsync(destination);
        }

        /// <summary>
        /// Creates a temp folder in the given directory path
        /// </summary>
        /// <param name="directoryPath">Directory to create the temp folder within</param>
        /// <returns>New temp folder path</returns>
        public Task<string> CreateTempFolderAsync(string directoryPath)
        {
            // if we have already created the temp folder, reuse it
            if(_tempFolderBase.IsSet() && _tempFolderPath.IsSet())
                return Task.FromResult(_tempFolderPath!);

            _tempFolderBase = directoryPath;
            _tempFolderPath = Path.Combine(directoryPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolderPath);
            return Task.FromResult(_tempFolderPath);
        }

        /// <summary>
        /// Deletes the existing temp folder
        /// </summary>
        /// <returns>Task</returns>
        public async Task DeleteTempFolderAsync()
        {
            // if we don't have a temp folder, return
            if(!_tempFolderPath.IsSet())
                return;

            await DeleteDirectoryAsync(_tempFolderPath!);
        }

        /// <summary>
        /// Creates a temp file with the given content and extension in
        /// our already created Temp Folder. If we haven't created a temp folder,
        /// throw exception.
        /// </summary>
        /// <param name="content">Content of file to write in temp file</param>
        /// <param name="extension">Extension of temp file to use</param>
        /// <returns>InputFile reference to new temp file</returns>
        public async Task<InputFile> CreateTempFileAsync(string content, string extension)
        {
            if(!_tempFolderPath.IsSet() || !_tempFolderBase.IsSet())
                throw new Exception("Temp folder must be created before Temp file");

            var filePath = Path.Combine(_tempFolderPath!, Guid.NewGuid().ToString() + extension);
            using (var writer = File.CreateText(filePath))
            {
                await writer.WriteLineAsync(content).ConfigureAwait(false);
            }

            return MapFileInfoToInputFile(new FileInfo(filePath), _tempFolderBase!);
        }

        /// <summary>
        /// Helper that maps a FileInfo to a new InputFile reference
        /// </summary>
        /// <param name="info">FileInfo</param>
        /// <param name="basePath">Base path used to determine relative paths</param>
        /// <returns>InputFile mapping</returns>
        private static InputFile MapFileInfoToInputFile(FileInfo info, string basePath){

            // ensure that we normalize our base path to remove any trailing slashes
            if(basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                basePath = basePath[0..^1];

            return new InputFile(
                Name: Path.GetFileNameWithoutExtension(info.Name),
                Extension: info.Extension,
                BaseDirectory: basePath,
                RelativeDirectory: info.DirectoryName!.Replace(basePath, "")
            );
        }
    }
}