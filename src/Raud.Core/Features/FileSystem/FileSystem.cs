using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Raud.Core.Extensions;
using Raud.Core.Features.Shared;

namespace Raud.Core.Features.FileSystem
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
        Task<string> CreateTempFileAsync(string content);

    }

    public class DiskFileSystem : IFileSystem
    {
        private string _tempFolderPath;

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
                Console.WriteLine("Writing Output File : " + file.FullPath);
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

        public async Task<string> CreateTempFileAsync(string content)
        {
            if(!this._tempFolderPath.IsSet())
                throw new Exception("Temp folder must be created before Temp file");

            var filePath = Path.Combine(this._tempFolderPath, Guid.NewGuid().ToString() + ".tmp");
            using (var writer = File.CreateText(filePath))
            {
                Console.WriteLine("Writing Temp File : " + filePath);
                await writer.WriteLineAsync(content).ConfigureAwait(false);
            }

            return filePath;
        }

        private InputFile MapFileInfoToInputFile(FileInfo info, string basePath){
            return new InputFile()
            {
                Name = Path.GetFileNameWithoutExtension(info.Name),
                Extension = info.Extension,
                FullDirectory = info.DirectoryName,
                FullPath = info.FullName,
                RelativeDirectory = info.DirectoryName.Replace(basePath, "")
            };
        }
    }
}