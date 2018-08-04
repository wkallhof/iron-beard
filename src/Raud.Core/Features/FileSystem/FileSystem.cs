using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Raud.Core.Extensions;
using Raud.Core.Features.Shared;
using SystemFile = System.IO.File;

namespace Raud.Core.Features.FileSystem
{
    public interface IFileSystem
    {
        ServiceResult<IEnumerable<File>> GetFiles(string path);
        Task<ServiceResult<bool>> WriteFilesAsync(List<File> files);
    }

    public class DiskFileSystem : IFileSystem
    {
        public ServiceResult<IEnumerable<File>> GetFiles(string path)
        {
            try{
                var directory = new DirectoryInfo(path);
                var files = directory.EnumerateFiles("*.*", SearchOption.AllDirectories).Select(this.MapFileInfoToFile);
                return new ServiceResult<IEnumerable<File>> { Data = files };
            }
            catch(Exception e){
                return new ServiceResult<IEnumerable<File>>(e.Message);
            }
        }

        public async Task<ServiceResult<bool>> WriteFilesAsync(List<File> files)
        {
            return new ServiceResult<bool>();
        }

        private File MapFileInfoToFile(FileInfo info){
            return new File()
            {
                Name = Path.GetFileNameWithoutExtension(info.Name),
                Extension = info.Extension,
                Directory = info.DirectoryName
            };
        }
    }
}