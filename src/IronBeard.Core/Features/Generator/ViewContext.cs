using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Features.Generator;

/// <summary>
/// Represents the context that is passed to each Razor view for templating.
/// </summary>
public class ViewContext
{
    /// <summary>
    /// This represents the current OutputFile being processed. Useful for pulling in
    /// current title, finding siblings, etc.
    /// </summary>
    public OutputFile Current {get;set;}

    /// <summary>
    /// All of the current file's directory siblings
    /// </summary>
    public IEnumerable<OutputFile> Siblings { get; set; }

    /// <summary>
    /// All of the current file's children (files in sub-directories)
    /// </summary>
    public IEnumerable<OutputFile> Children { get; set; }

    /// <summary>
    /// All files currently in the generator context. Useful for building up
    /// navigation, sitemaps, etc.
    /// </summary>
    public IEnumerable<OutputFile> All { get; set; }

    /// <summary>
    /// Current instance of the site's config values
    /// </summary>
    public BeardConfig Config { get; set; }

    public ViewContext(OutputFile current, GeneratorContext context, BeardConfig config){
        Current = current;

        Siblings = context.OutputFiles.Where(x => x.RelativeDirectory.Equals(current.RelativeDirectory) && x != current);
        Children = context.OutputFiles.Where(x => x.RelativeDirectory.Contains(current.RelativeDirectory) && !x.RelativeDirectory.Equals(current.RelativeDirectory));
        All = context.OutputFiles;
        Config = config;
    }
}