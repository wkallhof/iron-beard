using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Shared;
using NSubstitute;

namespace IronBeard.Core.Tests.Features.Generator;

public class StaticGeneratorTests
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly GeneratorContext _context;

    public StaticGeneratorTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _logger = Substitute.For<ILogger>();
        _context = new GeneratorContext("/input", "/output");
    }

    [Fact]
    public void Constructor_NullFileSystem_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new StaticGenerator(null!, _logger, _context));
    }

    [Fact]
    public void Constructor_EmptyInputDirectory_ThrowsArgumentException()
    {
        var context = new GeneratorContext("", "/output");
        Assert.Throws<ArgumentException>(() =>
            new StaticGenerator(_fileSystem, _logger, context));
    }

    [Fact]
    public void Constructor_EmptyOutputDirectory_ThrowsArgumentException()
    {
        var context = new GeneratorContext("/input", "");
        Assert.Throws<ArgumentException>(() =>
            new StaticGenerator(_fileSystem, _logger, context));
    }

    [Fact]
    public async Task Generate_NoProcessors_ThrowsException()
    {
        var generator = new StaticGenerator(_fileSystem, _logger, _context);

        await Assert.ThrowsAsync<Exception>(() => generator.Generate());
    }

    [Fact]
    public async Task Generate_CallsPreProcessProcessPostProcessOnAllFiles()
    {
        var processor = Substitute.For<IProcessor>();
        var inputFile = new InputFile("page", ".md", "/input", "/blog");
        var outputFile = new OutputFile(inputFile, "/output") { Content = "html" };

        _fileSystem.GetFiles("/input").Returns(new[] { inputFile });
        _fileSystem.CreateTempFolderAsync("/input").Returns(Task.FromResult("/tmp"));
        processor.ProcessAsync(inputFile).Returns(Task.FromResult<OutputFile?>(outputFile));

        var generator = new StaticGenerator(_fileSystem, _logger, _context);
        generator.AddProcessor(processor);

        await generator.Generate();

        await processor.Received(1).PreProcessAsync(inputFile);
        await processor.Received(1).ProcessAsync(inputFile);
        await processor.Received(1).PostProcessAsync(outputFile);
    }

    [Fact]
    public async Task Generate_WritesOutputFiles()
    {
        var processor = Substitute.For<IProcessor>();
        var inputFile = new InputFile("page", ".md", "/input", "/blog");
        var outputFile = new OutputFile(inputFile, "/output") { Content = "html" };

        _fileSystem.GetFiles("/input").Returns(new[] { inputFile });
        _fileSystem.CreateTempFolderAsync("/input").Returns(Task.FromResult("/tmp"));
        processor.ProcessAsync(inputFile).Returns(Task.FromResult<OutputFile?>(outputFile));

        var generator = new StaticGenerator(_fileSystem, _logger, _context);
        generator.AddProcessor(processor);

        await generator.Generate();

        await _fileSystem.Received(1).WriteOutputFilesAsync(Arg.Is<IEnumerable<OutputFile>>(
            files => files.Contains(outputFile)));
    }

    [Fact]
    public async Task Generate_DeletesTempFolderInFinally()
    {
        var processor = Substitute.For<IProcessor>();
        _fileSystem.GetFiles("/input").Returns(new[] { new InputFile("page", ".md", "/input", "/") });
        _fileSystem.CreateTempFolderAsync("/input").Returns(Task.FromResult("/tmp"));
        processor.ProcessAsync(Arg.Any<InputFile>()).Returns(Task.FromResult<OutputFile?>(null));

        var generator = new StaticGenerator(_fileSystem, _logger, _context);
        generator.AddProcessor(processor);

        await generator.Generate();

        await _fileSystem.Received(1).DeleteTempFolderAsync();
    }

    [Fact]
    public async Task Generate_DeletesTempFolder_EvenWhenProcessorThrows()
    {
        var processor = Substitute.For<IProcessor>();
        _fileSystem.GetFiles("/input").Returns(new[] { new InputFile("page", ".md", "/input", "/") });
        _fileSystem.CreateTempFolderAsync("/input").Returns(Task.FromResult("/tmp"));
        processor.PreProcessAsync(Arg.Any<InputFile>()).Returns(Task.FromException(new InvalidOperationException("test error")));

        var generator = new StaticGenerator(_fileSystem, _logger, _context);
        generator.AddProcessor(processor);

        await Assert.ThrowsAsync<InvalidOperationException>(() => generator.Generate());

        await _fileSystem.Received(1).DeleteTempFolderAsync();
    }

    [Fact]
    public async Task Generate_ClearsOutputDirectoryFirst()
    {
        var processor = Substitute.For<IProcessor>();
        _fileSystem.GetFiles("/input").Returns(Array.Empty<InputFile>());
        _fileSystem.CreateTempFolderAsync("/input").Returns(Task.FromResult("/tmp"));

        var generator = new StaticGenerator(_fileSystem, _logger, _context);
        generator.AddProcessor(processor);

        await generator.Generate();

        await _fileSystem.Received(1).DeleteDirectoryAsync("/output");
    }
}
