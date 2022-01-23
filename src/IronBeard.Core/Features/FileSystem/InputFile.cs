namespace IronBeard.Core.Features.FileSystem;

/// <summary>
/// This represents a file that was scanned with the static generator.
/// </summary>
public record InputFile(string Name, string Extension, string BaseDirectory, string RelativeDirectory)
{
    public string FullDirectory => Path.GetFullPath(BaseDirectory + RelativeDirectory);
    public string FullPath => Path.Combine(FullDirectory, Name + Extension);
    public string RelativePath => Path.Combine(RelativeDirectory, Name + Extension);
}