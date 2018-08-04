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
    }

    public class DiskFileSystem : IFileSystem
    {
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
            Directory.CreateDirectory(file.FullDirectory);
            using (var writer = File.CreateText(file.FullPath))
            {
                Console.WriteLine("Writing Output File : " + file.FullPath);
                await writer.WriteLineAsync(file.Content).ConfigureAwait(false);
            } 
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