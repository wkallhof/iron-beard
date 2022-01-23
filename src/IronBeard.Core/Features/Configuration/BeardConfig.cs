namespace IronBeard.Core.Features.Configuration;

/// <summary>
/// Main configuration settings, parsed from beard.json file.
/// </summary>
public class BeardConfig
{
    public string SiteTitle { get; set; } = "My IronBeard Site";
    public string IndexFileName { get; set; } = "Index";
    public string LayoutFileName { get; set; } = "_Layout";
    public List<string> StaticExtensionIgnoreList { get; set; } = new List<string> { ".cshtml", ".md", ".DS_Store", ".json" };
    public bool ExcludeHtmlExtension { get; set; } = true;
    public bool EnableMarkdownExtensions { get; set; } = false;
}