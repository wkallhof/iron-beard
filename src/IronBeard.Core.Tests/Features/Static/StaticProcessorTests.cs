using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Static;
using NSubstitute;

namespace IronBeard.Core.Tests.Features.Static;

public class StaticProcessorTests
{
    private readonly ILogger _logger;
    private readonly IUrlProvider _urlProvider;
    private readonly BeardConfig _config;
    private readonly GeneratorContext _context;
    private readonly StaticProcessor _processor;

    public StaticProcessorTests()
    {
        _logger = Substitute.For<ILogger>();
        _urlProvider = Substitute.For<IUrlProvider>();
        _config = new BeardConfig();
        _context = new GeneratorContext("/input", "/output");
        _processor = new StaticProcessor(_logger, _urlProvider, _config, _context);
    }

    [Theory]
    [InlineData(".cshtml")]
    [InlineData(".md")]
    [InlineData(".json")]
    [InlineData(".DS_Store")]
    public async Task ProcessAsync_IgnoredExtension_ReturnsNull(string extension)
    {
        var file = new InputFile("file", extension, "/input", "/");
        var result = await _processor.ProcessAsync(file);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(".CSHTML")]
    [InlineData(".Md")]
    [InlineData(".JSON")]
    public async Task ProcessAsync_IgnoredExtension_CaseInsensitive(string extension)
    {
        var file = new InputFile("file", extension, "/input", "/");
        var result = await _processor.ProcessAsync(file);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(".css")]
    [InlineData(".js")]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".svg")]
    public async Task ProcessAsync_StaticFile_ReturnsOutputWithDirectCopy(string extension)
    {
        var file = new InputFile("file", extension, "/input", "/assets");
        _urlProvider.GetUrl(file).Returns($"/assets/file{extension}");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.True(result!.DirectCopy);
    }

    [Fact]
    public async Task ProcessAsync_StaticFile_SetsUrlFromProvider()
    {
        var file = new InputFile("style", ".css", "/input", "/assets");
        _urlProvider.GetUrl(file).Returns("/assets/style.css");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Equal("/assets/style.css", result!.Url);
    }

    [Fact]
    public async Task ProcessAsync_StaticFile_UsesOutputDirectory()
    {
        var file = new InputFile("style", ".css", "/input", "/assets");
        _urlProvider.GetUrl(file).Returns("/assets/style.css");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Equal("/output", result!.BaseDirectory);
    }

    [Fact]
    public async Task PreProcessAsync_CompletesWithoutError()
    {
        var file = new InputFile("style", ".css", "/input", "/assets");
        await _processor.PreProcessAsync(file);
    }

    [Fact]
    public async Task PostProcessAsync_CompletesWithoutError()
    {
        var input = new InputFile("style", ".css", "/input", "/assets");
        var output = new OutputFile(input, "/output");
        await _processor.PostProcessAsync(output);
    }
}
