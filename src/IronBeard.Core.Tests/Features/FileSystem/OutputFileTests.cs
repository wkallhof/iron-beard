using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Tests.Features.FileSystem;

public class OutputFileTests
{
    [Fact]
    public void Constructor_CopiesFromInputFile()
    {
        var input = new InputFile("page", ".md", "/site", "/blog");
        var output = new OutputFile(input, "/output");

        Assert.Equal(input, output.Input);
        Assert.Equal("page", output.Name);
        Assert.Equal(".md", output.Extension);
        Assert.Equal("/output", output.BaseDirectory);
        Assert.Equal("/blog", output.RelativeDirectory);
    }

    [Fact]
    public void Defaults_MetadataEmpty_DirectCopyFalse_ContentNull_UrlNull()
    {
        var input = new InputFile("page", ".md", "/site", "/blog");
        var output = new OutputFile(input, "/output");

        Assert.Empty(output.Metadata);
        Assert.False(output.DirectCopy);
        Assert.Null(output.Content);
        Assert.Null(output.Url);
    }

    [Fact]
    public void FullPath_UsesBaseDirectoryNotInputBaseDirectory()
    {
        var input = new InputFile("page", ".md", "/site", "/blog");
        var output = new OutputFile(input, "/output");

        Assert.EndsWith(Path.Combine("output", "blog", "page.md"), output.FullPath);
        Assert.DoesNotContain("site", output.FullPath);
    }

    [Fact]
    public void Extension_CanBeChanged()
    {
        var input = new InputFile("page", ".md", "/site", "/blog");
        var output = new OutputFile(input, "/output") { Extension = ".html" };

        Assert.Equal(".html", output.Extension);
        Assert.EndsWith("page.html", output.FullPath);
    }
}
