using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;

namespace IronBeard.Core.Features.Shared
{
    public interface IProcessor
    {
        Task BeforeProcessAsync(InputFile file, GeneratorContext context);
        Task<OutputFile> ProcessAsync(InputFile file, GeneratorContext context);
        Task AfterProcessAsync(OutputFile file, GeneratorContext context);
    }
}