using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Features.Shared
{
    public interface IFileProcessor
    {
        Task<(bool processed, OutputFile file)> ProcessInputAsync(InputFile file, string outputDirectory);
        Task ProcessOutputAsync(OutputFile file);
    }
}