using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Shared;
using IronBeard.Core.Features.Logging;

namespace IronBeard.Core.Features.FileSystem
{
    public interface IFileSystem
    {
        IEnumerable<InputFile> GetFiles(string path);
        Task<string> ReadAllTextAsync(string path);
        Task WriteOutputFilesAsync(IEnumerable<OutputFile> files);
        Task WriteOutputFileAsync(OutputFile file);
        Task DeleteDirectoryAsync(string path);
        Task CopyOutputFileAsync(OutputFile file);

        Task<string> CreateTempFolderAsync(string basePath);
        Task DeleteTempFolderAsync();
        Task<InputFile> CreateTempFileAsync(string content);

    }

    public class DiskFileSystem : IFileSystem
    {
        private string _tempFolderPath;
        private string _tempFolderBase;
        private ILogger _log;

        public DiskFileSystem(ILogger logger){
            this._log = logger;
        }

        public IEnumerable<InputFile> GetFiles(string path)
        {
            var directory = new DirectoryInfo(path);
            var files = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Select(x => this.MapFileInfoToInputFile(x, path));
            return files;
        }

        public async Task<string> ReadAllTextAsync(string path)
        {
            const int fileBufferSize = 4096;
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, fileBufferSize, true))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            } 
        }

        public async Task WriteOutputFilesAsync(IEnumerable<OutputFile> files){
            foreach(var file in files){
                await this.WriteOutputFileAsync(file);
            }
        }

        public async Task WriteOutputFileAsync(OutputFile file){
            if(file.DirectCopy){
                await this.CopyOutputFileAsync(file);
                return;
            }
            
            Directory.CreateDirectory(file.FullDirectory);
            using (var writer = File.CreateText(file.FullPath))
            {
                //Console.WriteLine($"Writing Output File : {Path.Combine(file.RelativeDirectory, file.Name + file.Extension)}");
                await writer.WriteLineAsync(file.Content).ConfigureAwait(false);
            } 
        }

        public Task DeleteDirectoryAsync(string path){
            if(Directory.Exists(path))
                Directory.Delete(path, recursive: true);
            return Task.CompletedTask;
        }

        public async Task CopyOutputFileAsync(OutputFile file){
            Directory.CreateDirectory(file.FullDirectory);
            using (Stream source = File.Open(file.Input.FullPath, FileMode.Open))
            using(Stream destination = File.Create(file.FullPath))
            {
                await source.CopyToAsync(destination);
            }
        }

        public Task<string> CreateTempFolderAsync(string basePath)
        {
            if(this._tempFolderBase.IsSet() && this._tempFolderPath.IsSet())
                return Task.FromResult(this._tempFolderPath);

            this._tempFolderBase = basePath;
            this._tempFolderPath = Path.Combine(basePath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(this._tempFolderPath);
            return Task.FromResult(this._tempFolderPath);
        }

        public async Task DeleteTempFolderAsync()
        {
            if(!this._tempFolderPath.IsSet())
                return;

            await this.DeleteDirectoryAsync(this._tempFolderPath);
        }

        public async Task<InputFile> CreateTempFileAsync(string content)
        {
            if(!this._tempFolderPath.IsSet())
                throw new Exception("Temp folder must be created before Temp file");

            var filePath = Path.Combine(this._tempFolderPath, Guid.NewGuid().ToString() + ".tmp");
            using (var writer = File.CreateText(filePath))
            {
                await writer.WriteLineAsync(content).ConfigureAwait(false);
            }

            return this.MapFileInfoToInputFile(new FileInfo(filePath), this._tempFolderBase);
        }

        private InputFile MapFileInfoToInputFile(FileInfo info, string basePath){
            return new InputFile()
            {
                Name = Path.GetFileNameWithoutExtension(info.Name),
                Extension = info.Extension,
                BaseDirectory = basePath,
                RelativeDirectory = info.DirectoryName.Replace(basePath, "")
            };
        }
    }
}