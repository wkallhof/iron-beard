using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Razor;
using IronBeard.Core.Features.Routing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IronBeard.Core.Tests.Features.Razor;

public class RazorProcessorTests
{
    private readonly IFileSystem _fileSystem;
    private readonly IRazorViewRenderer _renderer;
    private readonly ILogger _logger;
    private readonly IUrlProvider _urlProvider;
    private readonly BeardConfig _config;
    private readonly GeneratorContext _context;
    private readonly RazorProcessor _processor;

    public RazorProcessorTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _renderer = Substitute.For<IRazorViewRenderer>();
        _logger = Substitute.For<ILogger>();
        _urlProvider = Substitute.For<IUrlProvider>();
        _config = new BeardConfig();
        _context = new GeneratorContext("/input", "/output");
        _processor = new RazorProcessor(_fileSystem, _renderer, _logger, _urlProvider, _config, _context);
    }

    // --- PreProcessAsync ---

    [Fact]
    public async Task PreProcessAsync_LayoutFile_SetsContextLayout()
    {
        var file = new InputFile("_Layout", ".cshtml", "/input", "/");

        await _processor.PreProcessAsync(file);

        Assert.Equal(file, _context.Layout);
    }

    [Fact]
    public async Task PreProcessAsync_LayoutFileCaseInsensitive_SetsContextLayout()
    {
        var file = new InputFile("_layout", ".cshtml", "/input", "/");

        await _processor.PreProcessAsync(file);

        Assert.Equal(file, _context.Layout);
    }

    [Fact]
    public async Task PreProcessAsync_NonCshtmlFile_DoesNotSetLayout()
    {
        var file = new InputFile("_Layout", ".md", "/input", "/");

        await _processor.PreProcessAsync(file);

        Assert.Null(_context.Layout);
    }

    [Fact]
    public async Task PreProcessAsync_NonLayoutCshtmlFile_DoesNotSetLayout()
    {
        var file = new InputFile("page", ".cshtml", "/input", "/");

        await _processor.PreProcessAsync(file);

        Assert.Null(_context.Layout);
    }

    // --- ProcessAsync ---

    [Fact]
    public async Task ProcessAsync_NonCshtmlFile_ReturnsNull()
    {
        var file = new InputFile("page", ".md", "/input", "/");

        var result = await _processor.ProcessAsync(file);

        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessAsync_LayoutFile_ReturnsNull()
    {
        var layout = new InputFile("_Layout", ".cshtml", "/input", "/");
        _context.Layout = layout;

        var result = await _processor.ProcessAsync(layout);

        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessAsync_PartialFile_ReturnsNull()
    {
        var file = new InputFile("_Partial", ".cshtml", "/input", "/");

        var result = await _processor.ProcessAsync(file);

        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessAsync_ValidCshtmlFile_ReturnsOutputFile()
    {
        var file = new InputFile("page", ".cshtml", "/input", "/blog");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("@*META\ntitle: My Page\n*@\n<h1>Hello</h1>");
        _urlProvider.GetUrl(file).Returns("/blog/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Equal(".html", result!.Extension);
        Assert.Equal("/blog/page", result.Url);
    }

    [Fact]
    public async Task ProcessAsync_ExtractsYamlMetadata()
    {
        var file = new InputFile("page", ".cshtml", "/input", "/");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("@*META\ntitle: Test Title\nauthor: John\n*@\n<h1>Hi</h1>");
        _urlProvider.GetUrl(file).Returns("/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Equal("Test Title", result!.Metadata["title"]);
        Assert.Equal("John", result.Metadata["author"]);
    }

    [Fact]
    public async Task ProcessAsync_NoMetadata_EmptyMetadata()
    {
        var file = new InputFile("page", ".cshtml", "/input", "/");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("<h1>No metadata</h1>");
        _urlProvider.GetUrl(file).Returns("/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Empty(result!.Metadata);
    }

    [Fact]
    public async Task ProcessAsync_EmptyFileContent_ReturnsNull()
    {
        var file = new InputFile("page", ".cshtml", "/input", "/");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("");

        var result = await _processor.ProcessAsync(file);

        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessAsync_CshtmlExtensionCaseInsensitive()
    {
        var file = new InputFile("page", ".CSHTML", "/input", "/");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("<h1>Content</h1>");
        _urlProvider.GetUrl(file).Returns("/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ProcessAsync_InvalidYaml_LogsErrorAndReturnsOutput()
    {
        var file = new InputFile("page", ".cshtml", "/input", "/");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("@*META\n: invalid: yaml: content\n*@\n<h1>Content</h1>");
        _urlProvider.GetUrl(file).Returns("/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        _logger.Received().Error<RazorProcessor>(Arg.Is<string>(s => s.Contains("Error parsing YAML")));
    }

    [Fact]
    public async Task ProcessAsync_MetaStartWithoutEnd_EmptyMetadata()
    {
        var file = new InputFile("page", ".cshtml", "/input", "/");
        _fileSystem.ReadAllTextAsync(file.FullPath).Returns("@*META\ntitle: Test\n<h1>No closing tag</h1>");
        _urlProvider.GetUrl(file).Returns("/page");

        var result = await _processor.ProcessAsync(file);

        Assert.NotNull(result);
        Assert.Empty(result!.Metadata);
    }

    // --- PostProcessAsync ---

    [Fact]
    public async Task PostProcessAsync_MarkdownFile_CallsRenderer()
    {
        var input = new InputFile("page", ".md", "/input", "/blog");
        var layout = new InputFile("_Layout", ".cshtml", "/input", "/");
        _context.Layout = layout;

        var output = new OutputFile(input, "/output") { Content = "<p>Hello</p>" };
        var tempFile = new InputFile("temp", ".cshtml", "/input", "/tmp");

        _fileSystem.CreateTempFileAsync(Arg.Any<string>(), ".cshtml").Returns(tempFile);
        _renderer.RenderAsync(tempFile.RelativePath, Arg.Any<ViewContext>()).Returns("<html>rendered</html>");

        await _processor.PostProcessAsync(output);

        Assert.Equal("<html>rendered</html>", output.Content);
    }

    [Fact]
    public async Task PostProcessAsync_CshtmlFile_CallsRenderer()
    {
        var input = new InputFile("page", ".cshtml", "/input", "/blog");
        var layout = new InputFile("_Layout", ".cshtml", "/input", "/");
        _context.Layout = layout;

        var output = new OutputFile(input, "/output");
        var tempFile = new InputFile("temp", ".cshtml", "/input", "/tmp");

        _fileSystem.ReadAllTextAsync(input.FullPath).Returns("<h1>Razor Content</h1>");
        _fileSystem.CreateTempFileAsync(Arg.Any<string>(), ".cshtml").Returns(tempFile);
        _renderer.RenderAsync(tempFile.RelativePath, Arg.Any<ViewContext>()).Returns("<html>rendered razor</html>");

        await _processor.PostProcessAsync(output);

        Assert.Equal("<html>rendered razor</html>", output.Content);
    }

    [Fact]
    public async Task PostProcessAsync_NonCshtmlNonMdFile_DoesNothing()
    {
        var input = new InputFile("style", ".css", "/input", "/assets");
        var output = new OutputFile(input, "/output");

        await _processor.PostProcessAsync(output);

        await _renderer.DidNotReceive().RenderAsync(Arg.Any<string>(), Arg.Any<ViewContext>());
    }

    [Fact]
    public async Task PostProcessAsync_LayoutFile_DoesNotProcess()
    {
        var layout = new InputFile("_Layout", ".cshtml", "/input", "/");
        _context.Layout = layout;
        var output = new OutputFile(layout, "/output");

        await _processor.PostProcessAsync(output);

        await _renderer.DidNotReceive().RenderAsync(Arg.Any<string>(), Arg.Any<ViewContext>());
    }

    [Fact]
    public async Task PostProcessAsync_PartialCshtmlFile_DoesNotProcess()
    {
        var partial = new InputFile("_Header", ".cshtml", "/input", "/");
        var output = new OutputFile(partial, "/output");

        await _processor.PostProcessAsync(output);

        await _renderer.DidNotReceive().RenderAsync(Arg.Any<string>(), Arg.Any<ViewContext>());
    }

    [Fact]
    public async Task PostProcessAsync_CshtmlFile_EmptyContent_DoesNotCallRenderer()
    {
        var input = new InputFile("page", ".cshtml", "/input", "/");
        var output = new OutputFile(input, "/output");

        _fileSystem.ReadAllTextAsync(input.FullPath).Returns("");

        await _processor.PostProcessAsync(output);

        await _renderer.DidNotReceive().RenderAsync(Arg.Any<string>(), Arg.Any<ViewContext>());
    }

    [Fact]
    public async Task PostProcessAsync_RendererThrows_WrapsExceptionWithFilePath()
    {
        var input = new InputFile("page", ".cshtml", "/input", "/blog");
        var layout = new InputFile("_Layout", ".cshtml", "/input", "/");
        _context.Layout = layout;
        var output = new OutputFile(input, "/output");
        var tempFile = new InputFile("temp", ".cshtml", "/input", "/tmp");

        _fileSystem.ReadAllTextAsync(input.FullPath).Returns("<h1>Content</h1>");
        _fileSystem.CreateTempFileAsync(Arg.Any<string>(), ".cshtml").Returns(tempFile);
        _renderer.RenderAsync(tempFile.RelativePath, Arg.Any<ViewContext>())
            .ThrowsAsync(new Exception("View not found: " + tempFile.FullPath));

        var ex = await Assert.ThrowsAsync<Exception>(() => _processor.PostProcessAsync(output));
        Assert.Contains(input.FullPath, ex.Message);
    }

    [Fact]
    public async Task PostProcessAsync_NoLayout_MarkdownRendersWithEmptyLayout()
    {
        // No layout set in context
        var input = new InputFile("page", ".md", "/input", "/");
        var output = new OutputFile(input, "/output") { Content = "<p>Hello</p>" };
        var tempFile = new InputFile("temp", ".cshtml", "/input", "/tmp");

        _fileSystem.CreateTempFileAsync(Arg.Any<string>(), ".cshtml").Returns(tempFile);
        _renderer.RenderAsync(tempFile.RelativePath, Arg.Any<ViewContext>()).Returns("<html>no layout</html>");

        await _processor.PostProcessAsync(output);

        // When no layout, AppendLayoutInfo returns empty string
        await _fileSystem.Received().CreateTempFileAsync(Arg.Is<string>(s => s == string.Empty), ".cshtml");
    }

    [Fact]
    public async Task PostProcessAsync_WithLayout_AppendsLayoutDirective()
    {
        var input = new InputFile("page", ".md", "/input", "/");
        var layout = new InputFile("_Layout", ".cshtml", "/input", "/");
        _context.Layout = layout;

        var output = new OutputFile(input, "/output") { Content = "<p>Hello</p>" };
        var tempFile = new InputFile("temp", ".cshtml", "/input", "/tmp");

        _fileSystem.CreateTempFileAsync(Arg.Any<string>(), ".cshtml").Returns(tempFile);
        _renderer.RenderAsync(tempFile.RelativePath, Arg.Any<ViewContext>()).Returns("<html>rendered</html>");

        await _processor.PostProcessAsync(output);

        await _fileSystem.Received().CreateTempFileAsync(
            Arg.Is<string>(s => s.Contains("Layout") && s.Contains("<p>Hello</p>")),
            ".cshtml");
    }
}
