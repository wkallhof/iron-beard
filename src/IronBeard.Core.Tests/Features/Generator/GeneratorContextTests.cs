using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;

namespace IronBeard.Core.Tests.Features.Generator;

public class GeneratorContextTests
{
    [Fact]
    public void Constructor_SetsInputDirectory()
    {
        var context = new GeneratorContext("/input", "/output");
        Assert.Equal("/input", context.InputDirectory);
    }

    [Fact]
    public void Constructor_SetsOutputDirectory()
    {
        var context = new GeneratorContext("/input", "/output");
        Assert.Equal("/output", context.OutputDirectory);
    }

    [Fact]
    public void Constructor_InitializesEmptyInputFiles()
    {
        var context = new GeneratorContext("/input", "/output");
        Assert.Empty(context.InputFiles);
    }

    [Fact]
    public void Constructor_InitializesEmptyOutputFiles()
    {
        var context = new GeneratorContext("/input", "/output");
        Assert.Empty(context.OutputFiles);
    }

    [Fact]
    public void Constructor_LayoutDefaultsToNull()
    {
        var context = new GeneratorContext("/input", "/output");
        Assert.Null(context.Layout);
    }

    [Fact]
    public void Layout_CanBeSet()
    {
        var context = new GeneratorContext("/input", "/output");
        var layout = new InputFile("_Layout", ".cshtml", "/input", "/");

        context.Layout = layout;

        Assert.Equal(layout, context.Layout);
    }

    [Fact]
    public void InputFiles_CanBeSet()
    {
        var context = new GeneratorContext("/input", "/output");
        var files = new List<InputFile> { new("page", ".md", "/input", "/") };

        context.InputFiles = files;

        Assert.Single(context.InputFiles);
    }

    [Fact]
    public void OutputFiles_CanBeSet()
    {
        var context = new GeneratorContext("/input", "/output");
        var input = new InputFile("page", ".md", "/input", "/");
        var output = new OutputFile(input, "/output");

        context.OutputFiles = new List<OutputFile> { output };

        Assert.Single(context.OutputFiles);
    }
}
