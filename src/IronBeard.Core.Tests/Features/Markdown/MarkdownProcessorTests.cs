using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Markdown;
using IronBeard.Core.Features.Routing;
using NSubstitute;

namespace IronBeard.Core.Tests.Features.Markdown;

public class MarkdownProcessorTests
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IUrlProvider _urlProvider;
    private readonly BeardConfig _config;
    private readonly GeneratorContext _context;
    private readonly MarkdownProcessor _processor;

    public MarkdownProcessorTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _logger = Substitute.For<ILogger>();
        _urlProvider = Substitute.For<IUrlProvider>();
        _config = new BeardConfig();
        _context = new GeneratorContext("/input", "/output");
        _processor = new MarkdownProcessor(_fileSystem, _logger, _urlProvider, _config, _context);
    }

    [Fact]
    public async Task ProcessAsync_NonMarkdownFile_ReturnsNull()
    {
        var file = new InputFile("style", ".css", "/input", "/assets");
        var result = await _processor.ProcessAsync(file);
        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessAsync_MarkdownFile_ConvertsToHtml()
    {
        var file = new InputFile("page", ".md", "/input", "/blog");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("# Hello World");
        _urlProvider.GetUrl(file).Returns("/blog/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Contains("<h1>Hello World</h1>", result!.Content);
        Assert.Equal(".html", result.Extension);
    }

    [Fact]
    public async Task ProcessAsync_ExtractsYamlFrontmatter()
    {
        var file = new InputFile("page", ".md", "/input", "/blog");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("---\ntitle: My Post\nauthor: Test\n---\n# Content");
        _urlProvider.GetUrl(file).Returns("/blog/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Equal("My Post", result!.Metadata["title"]);
        Assert.Equal("Test", result.Metadata["author"]);
    }

    [Fact]
    public async Task ProcessAsync_NoYamlFrontmatter_EmptyMetadata()
    {
        var file = new InputFile("page", ".md", "/input", "/blog");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("# Just Content");
        _urlProvider.GetUrl(file).Returns("/blog/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Empty(result!.Metadata);
    }

    [Fact]
    public async Task ProcessAsync_SetsUrlFromUrlProvider()
    {
        var file = new InputFile("page", ".md", "/input", "/blog");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("# Content");
        _urlProvider.GetUrl(file).Returns("/blog/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Equal("/blog/page", result!.Url);
    }

    [Fact]
    public async Task ProcessAsync_EmptyFileContent_ReturnsNull()
    {
        var file = new InputFile("empty", ".md", "/input", "/blog");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("");

        var result = await _processor.ProcessAsync(file);

        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessAsync_MdExtensionCaseInsensitive()
    {
        var file = new InputFile("page", ".MD", "/input", "/blog");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("# Content");
        _urlProvider.GetUrl(file).Returns("/blog/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ProcessAsync_InvalidYaml_LogsErrorAndReturnsContent()
    {
        var file = new InputFile("page", ".md", "/input", "/blog");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("---\n: invalid: yaml: content\n---\n# Content");
        _urlProvider.GetUrl(file).Returns("/blog/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        _logger.Received().Error<MarkdownProcessor>(Arg.Is<string>(s => s.Contains("Error parsing YAML")));
    }

    [Fact]
    public async Task PreProcessAsync_CompletesWithoutError()
    {
        var file = new InputFile("page", ".md", "/input", "/blog");
        await _processor.PreProcessAsync(file);
    }

    [Fact]
    public async Task PostProcessAsync_CompletesWithoutError()
    {
        var input = new InputFile("page", ".md", "/input", "/blog");
        var output = new OutputFile(input, "/output");
        await _processor.PostProcessAsync(output);
    }
}
