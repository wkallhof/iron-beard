using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Routing;

namespace IronBeard.Core.Tests.Features.Routing;

public class UrlProviderTests
{
    private readonly BeardConfig _config = new();
    private readonly UrlProvider _urlProvider;

    public UrlProviderTests()
    {
        _urlProvider = new UrlProvider(_config);
    }

    [Fact]
    public void GetUrl_IndexFile_ReturnsDirectoryOnly()
    {
        var file = new InputFile("Index", ".md", "/site", "/blog");
        var url = _urlProvider.GetUrl(file);
        Assert.Equal("/blog", url);
    }

    [Fact]
    public void GetUrl_IndexFile_CaseInsensitive()
    {
        var file = new InputFile("index", ".md", "/site", "/blog");
        var url = _urlProvider.GetUrl(file);
        Assert.Equal("/blog", url);
    }

    [Fact]
    public void GetUrl_MarkdownFile_ExcludeHtmlExtensionTrue_ReturnsWithoutExtension()
    {
        var file = new InputFile("about", ".md", "/site", "/pages");
        var url = _urlProvider.GetUrl(file);
        Assert.Equal(Path.Combine("/pages", "about"), url);
    }

    [Fact]
    public void GetUrl_CshtmlFile_ExcludeHtmlExtensionTrue_ReturnsWithoutExtension()
    {
        var file = new InputFile("contact", ".cshtml", "/site", "/pages");
        var url = _urlProvider.GetUrl(file);
        Assert.Equal(Path.Combine("/pages", "contact"), url);
    }

    [Fact]
    public void GetUrl_MarkdownFile_ExcludeHtmlExtensionFalse_ReturnsWithHtmlExtension()
    {
        var config = new BeardConfig { ExcludeHtmlExtension = false };
        var provider = new UrlProvider(config);
        var file = new InputFile("about", ".md", "/site", "/pages");
        var url = provider.GetUrl(file);
        Assert.Equal(Path.Combine("/pages", "about.html"), url);
    }

    [Fact]
    public void GetUrl_StaticFile_KeepsOriginalExtension()
    {
        var file = new InputFile("style", ".css", "/site", "/assets");
        var url = _urlProvider.GetUrl(file);
        Assert.Equal(Path.Combine("/assets", "style.css"), url);
    }

    [Fact]
    public void GetUrl_JsFile_KeepsOriginalExtension()
    {
        var file = new InputFile("app", ".js", "/site", "/assets");
        var url = _urlProvider.GetUrl(file);
        Assert.Equal(Path.Combine("/assets", "app.js"), url);
    }

    [Fact]
    public void GetUrl_ImageFile_KeepsOriginalExtension()
    {
        var file = new InputFile("logo", ".png", "/site", "/images");
        var url = _urlProvider.GetUrl(file);
        Assert.Equal(Path.Combine("/images", "logo.png"), url);
    }

    [Fact]
    public void GetUrl_RootIndexFile_ReturnsRootDirectory()
    {
        var file = new InputFile("Index", ".cshtml", "/site", "/");
        var url = _urlProvider.GetUrl(file);
        Assert.Equal("/", url);
    }
}
