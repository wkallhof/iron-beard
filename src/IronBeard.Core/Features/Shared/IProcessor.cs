using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;

namespace IronBeard.Core.Features.Shared
{
    public interface IProcessor
    {
        Task PreProcessAsync(InputFile file, GeneratorContext context);
        Task<OutputFile> ProcessAsync(InputFile file, GeneratorContext context);
        Task PostProcessAsync(OutputFile file, GeneratorContext context);
    }
}