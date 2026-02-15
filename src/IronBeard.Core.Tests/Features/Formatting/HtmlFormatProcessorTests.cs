using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Formatting;
using IronBeard.Core.Features.Logging;
using NSubstitute;

namespace IronBeard.Core.Tests.Features.Formatting;

public class HtmlFormatProcessorTests
{
    private readonly ILogger _logger;
    private readonly HtmlFormatProcessor _processor;

    public HtmlFormatProcessorTests()
    {
        _logger = Substitute.For<ILogger>();
        _processor = new HtmlFormatProcessor(_logger);
    }

    [Fact]
    public async Task PostProcessAsync_ValidHtml_FormatsContent()
    {
        var input = new InputFile("page", ".html", "/input", "/");
        var output = new OutputFile(input, "/output") { Extension = ".html", Content = "<div><p>Hello</p></div>" };

        await _processor.PostProcessAsync(output);

        Assert.Contains("<div>", output.Content);
        Assert.Contains("<p>Hello</p>", output.Content);
    }

    [Fact]
    public async Task PostProcessAsync_NonHtmlFile_SkipsFormatting()
    {
        var input = new InputFile("style", ".css", "/input", "/");
        var output = new OutputFile(input, "/output") { Extension = ".css", Content = "body { color: red; }" };
        var originalContent = output.Content;

        await _processor.PostProcessAsync(output);

        Assert.Equal(originalContent, output.Content);
    }

    [Fact]
    public async Task PostProcessAsync_NullContent_SkipsFormatting()
    {
        var input = new InputFile("page", ".html", "/input", "/");
        var output = new OutputFile(input, "/output") { Extension = ".html", Content = null };

        await _processor.PostProcessAsync(output);

        Assert.Null(output.Content);
    }

    [Fact]
    public async Task PostProcessAsync_EmptyContent_SkipsFormatting()
    {
        var input = new InputFile("page", ".html", "/input", "/");
        var output = new OutputFile(input, "/output") { Extension = ".html", Content = "" };

        await _processor.PostProcessAsync(output);

        Assert.Equal("", output.Content);
    }

    [Fact]
    public async Task PostProcessAsync_MalformedHtml_LogsWarningWithoutThrowing()
    {
        var input = new InputFile("page", ".html", "/input", "/");
        var output = new OutputFile(input, "/output") { Extension = ".html", Content = "<div><p>unclosed" };

        await _processor.PostProcessAsync(output);

        _logger.Received().Warn<HtmlFormatProcessor>(Arg.Is<string>(s => s.Contains("isn't well formed")));
    }

    [Fact]
    public async Task ProcessAsync_AlwaysReturnsNull()
    {
        var file = new InputFile("page", ".html", "/input", "/");
        var result = await _processor.ProcessAsync(file);
        Assert.Null(result);
    }

    [Fact]
    public async Task PreProcessAsync_CompletesWithoutError()
    {
        var file = new InputFile("page", ".html", "/input", "/");
        await _processor.PreProcessAsync(file);
    }
}
