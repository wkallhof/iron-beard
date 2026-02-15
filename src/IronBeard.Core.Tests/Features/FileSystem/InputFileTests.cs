using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Tests.Features.FileSystem;

public class InputFileTests
{
    [Fact]
    public void FullDirectory_CombinesBaseAndRelativeDirectory()
    {
        var file = new InputFile("page", ".md", "/site", "/blog");
        Assert.EndsWith(Path.Combine("site", "blog"), file.FullDirectory);
    }

    [Fact]
    public void FullPath_CombinesFullDirectoryAndNameAndExtension()
    {
        var file = new InputFile("page", ".md", "/site", "/blog");
        Assert.EndsWith(Path.Combine("site", "blog", "page.md"), file.FullPath);
    }

    [Fact]
    public void RelativePath_CombinesRelativeDirectoryAndNameAndExtension()
    {
        var file = new InputFile("page", ".md", "/site", "/blog");
        Assert.Equal(Path.Combine("/blog", "page.md"), file.RelativePath);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var file1 = new InputFile("page", ".md", "/site", "/blog");
        var file2 = new InputFile("page", ".md", "/site", "/blog");
        Assert.Equal(file1, file2);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var file1 = new InputFile("page", ".md", "/site", "/blog");
        var file2 = new InputFile("other", ".md", "/site", "/blog");
        Assert.NotEqual(file1, file2);
    }
}
