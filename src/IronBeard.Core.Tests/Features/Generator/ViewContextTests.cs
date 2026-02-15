using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;

namespace IronBeard.Core.Tests.Features.Generator;

public class ViewContextTests
{
    private readonly BeardConfig _config = new();
    private readonly GeneratorContext _generatorContext;

    public ViewContextTests()
    {
        _generatorContext = new GeneratorContext("/input", "/output");
    }

    [Fact]
    public void Siblings_ExcludesCurrent_IncludesSameDirectory()
    {
        var current = new OutputFile(new InputFile("page1", ".html", "/input", "/blog"), "/output");
        var sibling = new OutputFile(new InputFile("page2", ".html", "/input", "/blog"), "/output");
        var other = new OutputFile(new InputFile("page3", ".html", "/input", "/other"), "/output");

        _generatorContext.OutputFiles = new List<OutputFile> { current, sibling, other };

        var viewContext = new ViewContext(current, _generatorContext, _config);

        Assert.DoesNotContain(current, viewContext.Siblings);
        Assert.Contains(sibling, viewContext.Siblings);
        Assert.DoesNotContain(other, viewContext.Siblings);
    }

    [Fact]
    public void Children_IncludesSubdirectories_ExcludesSameDirectory()
    {
        var current = new OutputFile(new InputFile("index", ".html", "/input", "/blog"), "/output");
        var child = new OutputFile(new InputFile("post", ".html", "/input", "/blog/2024"), "/output");
        var sibling = new OutputFile(new InputFile("about", ".html", "/input", "/blog"), "/output");

        _generatorContext.OutputFiles = new List<OutputFile> { current, child, sibling };

        var viewContext = new ViewContext(current, _generatorContext, _config);

        Assert.Contains(child, viewContext.Children);
        Assert.DoesNotContain(sibling, viewContext.Children);
        Assert.DoesNotContain(current, viewContext.Children);
    }

    [Fact]
    public void All_ReturnsAllOutputFiles()
    {
        var current = new OutputFile(new InputFile("page1", ".html", "/input", "/blog"), "/output");
        var other = new OutputFile(new InputFile("page2", ".html", "/input", "/other"), "/output");

        _generatorContext.OutputFiles = new List<OutputFile> { current, other };

        var viewContext = new ViewContext(current, _generatorContext, _config);

        Assert.Equal(2, viewContext.All.Count());
        Assert.Contains(current, viewContext.All);
        Assert.Contains(other, viewContext.All);
    }

    [Fact]
    public void Config_IsSetFromConstructor()
    {
        var current = new OutputFile(new InputFile("page1", ".html", "/input", "/blog"), "/output");
        _generatorContext.OutputFiles = new List<OutputFile> { current };

        var viewContext = new ViewContext(current, _generatorContext, _config);

        Assert.Same(_config, viewContext.Config);
    }

    [Fact]
    public void Current_IsSetFromConstructor()
    {
        var current = new OutputFile(new InputFile("page1", ".html", "/input", "/blog"), "/output");
        _generatorContext.OutputFiles = new List<OutputFile> { current };

        var viewContext = new ViewContext(current, _generatorContext, _config);

        Assert.Same(current, viewContext.Current);
    }
}
