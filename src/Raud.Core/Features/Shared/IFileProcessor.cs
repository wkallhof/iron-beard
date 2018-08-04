using System.Threading.Tasks;
using Raud.Core.Features.FileSystem;

namespace Raud.Core.Features.Shared
{
    public interface IFileProcessor
    {
        Task<(bool processed, OutputFile file)> ProcessInputAsync(InputFile file, string outputDirectory);
        Task ProcessOutputAsync(OutputFile file);
    }
}