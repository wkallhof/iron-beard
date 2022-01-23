using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Features.Shared;

/// <summary>
/// Main processor interface. Allows for our three stages of processing
/// </summary>
public interface IProcessor
{
    Task PreProcessAsync(InputFile file);
    Task<OutputFile?> ProcessAsync(InputFile file);
    Task PostProcessAsync(OutputFile file);
}