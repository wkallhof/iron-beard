using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Logging;
using NSubstitute;

namespace IronBeard.Core.Tests.Features.FileSystem;

public class DiskFileSystemTests : IDisposable
{
    private readonly ILogger _logger;
    private readonly BeardConfig _config;
    private readonly DiskFileSystem _fileSystem;
    private readonly string _testDir;

    public DiskFileSystemTests()
    {
        _logger = Substitute.For<ILogger>();
        _config = new BeardConfig();
        _fileSystem = new DiskFileSystem(_logger, _config);
        _testDir = Path.Combine(Path.GetTempPath(), "ironbeard_test_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    // --- GetFiles ---

    [Fact]
    public void GetFiles_ReturnsAllFiles()
    {
        File.WriteAllText(Path.Combine(_testDir, "page.md"), "# Hello");
        File.WriteAllText(Path.Combine(_testDir, "style.css"), "body {}");

        var files = _fileSystem.GetFiles(_testDir).ToList();

        Assert.Equal(2, files.Count);
    }

    [Fact]
    public void GetFiles_IncludesSubdirectoryFiles()
    {
        var subDir = Path.Combine(_testDir, "blog");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(_testDir, "index.md"), "# Home");
        File.WriteAllText(Path.Combine(subDir, "post.md"), "# Post");

        var files = _fileSystem.GetFiles(_testDir).ToList();

        Assert.Equal(2, files.Count);
    }

    [Fact]
    public void GetFiles_MapsInputFileCorrectly()
    {
        File.WriteAllText(Path.Combine(_testDir, "page.md"), "content");

        var file = _fileSystem.GetFiles(_testDir).First();

        Assert.Equal("page", file.Name);
        Assert.Equal(".md", file.Extension);
        Assert.Equal(_testDir, file.BaseDirectory);
    }

    [Fact]
    public void GetFiles_SubdirectoryFile_HasCorrectRelativeDirectory()
    {
        var subDir = Path.Combine(_testDir, "blog");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "post.md"), "content");

        var file = _fileSystem.GetFiles(_testDir).First();

        Assert.Contains("blog", file.RelativeDirectory);
    }

    // --- ReadAllTextAsync ---

    [Fact]
    public async Task ReadAllTextAsync_ReadsFileContent()
    {
        var filePath = Path.Combine(_testDir, "test.txt");
        await File.WriteAllTextAsync(filePath, "Hello World");

        var content = await _fileSystem.ReadAllTextAsync(filePath);

        Assert.Equal("Hello World", content);
    }

    [Fact]
    public async Task ReadAllTextAsync_ReadsUtf8Content()
    {
        var filePath = Path.Combine(_testDir, "utf8.txt");
        await File.WriteAllTextAsync(filePath, "Hellesentlig");

        var content = await _fileSystem.ReadAllTextAsync(filePath);

        Assert.Contains("Hellesentlig", content);
    }

    // --- WriteOutputFileAsync ---

    [Fact]
    public async Task WriteOutputFileAsync_WritesContentToDisk()
    {
        _config.ExcludeHtmlExtension = false;
        var input = new InputFile("page", ".md", _testDir, "/");
        var output = new OutputFile(input, _testDir)
        {
            Extension = ".html",
            Content = "<h1>Hello</h1>"
        };

        await _fileSystem.WriteOutputFileAsync(output);

        Assert.True(File.Exists(output.FullPath));
        var content = await File.ReadAllTextAsync(output.FullPath);
        Assert.Contains("<h1>Hello</h1>", content);
    }

    [Fact]
    public async Task WriteOutputFileAsync_CreatesDirectoryIfNeeded()
    {
        _config.ExcludeHtmlExtension = false;
        var outputDir = Path.Combine(_testDir, "output");
        var input = new InputFile("page", ".md", _testDir, "/sub");
        var output = new OutputFile(input, outputDir)
        {
            Extension = ".html",
            Content = "<h1>Hello</h1>"
        };

        await _fileSystem.WriteOutputFileAsync(output);

        Assert.True(File.Exists(output.FullPath));
    }

    [Fact]
    public async Task WriteOutputFileAsync_ExcludeHtmlExtension_RemovesExtension()
    {
        _config.ExcludeHtmlExtension = true;
        var input = new InputFile("about", ".cshtml", _testDir, "/");
        var output = new OutputFile(input, _testDir)
        {
            Extension = ".html",
            Content = "<h1>About</h1>"
        };

        await _fileSystem.WriteOutputFileAsync(output);

        // The file should be written without extension (name "about" with empty extension)
        var expectedPath = Path.Combine(_testDir, "about");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public async Task WriteOutputFileAsync_ExcludeHtmlExtension_KeepsIndexExtension()
    {
        _config.ExcludeHtmlExtension = true;
        var input = new InputFile("Index", ".cshtml", _testDir, "/");
        var output = new OutputFile(input, _testDir)
        {
            Extension = ".html",
            Content = "<h1>Home</h1>"
        };

        await _fileSystem.WriteOutputFileAsync(output);

        var expectedPath = Path.Combine(_testDir, "Index.html");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public async Task WriteOutputFileAsync_DirectCopyTrue_CopiesFile()
    {
        // Create a source file
        var sourceDir = Path.Combine(_testDir, "source");
        Directory.CreateDirectory(sourceDir);
        var sourcePath = Path.Combine(sourceDir, "image.png");
        await File.WriteAllBytesAsync(sourcePath, new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        var input = new InputFile("image", ".png", sourceDir, "/");
        var outputDir = Path.Combine(_testDir, "output");
        var output = new OutputFile(input, outputDir) { DirectCopy = true };

        await _fileSystem.WriteOutputFileAsync(output);

        Assert.True(File.Exists(output.FullPath));
        var bytes = await File.ReadAllBytesAsync(output.FullPath);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, bytes);
    }

    // --- WriteOutputFilesAsync ---

    [Fact]
    public async Task WriteOutputFilesAsync_WritesMultipleFiles()
    {
        _config.ExcludeHtmlExtension = false;
        var input1 = new InputFile("page1", ".md", _testDir, "/");
        var input2 = new InputFile("page2", ".md", _testDir, "/");
        var output1 = new OutputFile(input1, _testDir) { Extension = ".html", Content = "one" };
        var output2 = new OutputFile(input2, _testDir) { Extension = ".html", Content = "two" };

        await _fileSystem.WriteOutputFilesAsync(new[] { output1, output2 });

        Assert.True(File.Exists(output1.FullPath));
        Assert.True(File.Exists(output2.FullPath));
    }

    // --- DeleteDirectoryAsync ---

    [Fact]
    public async Task DeleteDirectoryAsync_RemovesDirectory()
    {
        var dirToDelete = Path.Combine(_testDir, "to_delete");
        Directory.CreateDirectory(dirToDelete);
        File.WriteAllText(Path.Combine(dirToDelete, "file.txt"), "content");

        await _fileSystem.DeleteDirectoryAsync(dirToDelete);

        Assert.False(Directory.Exists(dirToDelete));
    }

    [Fact]
    public async Task DeleteDirectoryAsync_NonExistentDirectory_DoesNotThrow()
    {
        var nonExistent = Path.Combine(_testDir, "nonexistent");

        await _fileSystem.DeleteDirectoryAsync(nonExistent);
    }

    // --- CopyOutputFileAsync ---

    [Fact]
    public async Task CopyOutputFileAsync_CopiesFileToOutputLocation()
    {
        var sourcePath = Path.Combine(_testDir, "source.txt");
        await File.WriteAllTextAsync(sourcePath, "source content");

        var input = new InputFile("source", ".txt", _testDir, "/");
        var outputDir = Path.Combine(_testDir, "output");
        var output = new OutputFile(input, outputDir);

        await _fileSystem.CopyOutputFileAsync(output);

        Assert.True(File.Exists(output.FullPath));
        var content = await File.ReadAllTextAsync(output.FullPath);
        Assert.Equal("source content", content);
    }

    // --- CreateTempFolderAsync / DeleteTempFolderAsync ---

    [Fact]
    public async Task CreateTempFolderAsync_CreatesFolderAndReturnsPath()
    {
        var tempPath = await _fileSystem.CreateTempFolderAsync(_testDir);

        Assert.True(Directory.Exists(tempPath));
        Assert.StartsWith(_testDir, tempPath);

        // Cleanup
        Directory.Delete(tempPath, true);
    }

    [Fact]
    public async Task CreateTempFolderAsync_CalledTwice_ReturnsSamePath()
    {
        var first = await _fileSystem.CreateTempFolderAsync(_testDir);
        var second = await _fileSystem.CreateTempFolderAsync(_testDir);

        Assert.Equal(first, second);

        // Cleanup
        Directory.Delete(first, true);
    }

    [Fact]
    public async Task DeleteTempFolderAsync_NoTempFolder_DoesNotThrow()
    {
        var fs = new DiskFileSystem(_logger, _config);
        await fs.DeleteTempFolderAsync();
    }

    [Fact]
    public async Task DeleteTempFolderAsync_RemovesTempFolder()
    {
        var tempPath = await _fileSystem.CreateTempFolderAsync(_testDir);
        Assert.True(Directory.Exists(tempPath));

        await _fileSystem.DeleteTempFolderAsync();

        Assert.False(Directory.Exists(tempPath));
    }

    // --- CreateTempFileAsync ---

    [Fact]
    public async Task CreateTempFileAsync_WithoutTempFolder_ThrowsException()
    {
        var fs = new DiskFileSystem(_logger, _config);

        await Assert.ThrowsAsync<Exception>(() => fs.CreateTempFileAsync("content", ".cshtml"));
    }

    [Fact]
    public async Task CreateTempFileAsync_CreatesFileWithContent()
    {
        await _fileSystem.CreateTempFolderAsync(_testDir);

        var tempFile = await _fileSystem.CreateTempFileAsync("<h1>Hello</h1>", ".cshtml");

        Assert.Equal(".cshtml", tempFile.Extension);
        var content = await File.ReadAllTextAsync(tempFile.FullPath);
        Assert.Contains("<h1>Hello</h1>", content);

        // Cleanup
        await _fileSystem.DeleteTempFolderAsync();
    }

    [Fact]
    public async Task CreateTempFileAsync_ReturnsInputFileWithCorrectBase()
    {
        await _fileSystem.CreateTempFolderAsync(_testDir);

        var tempFile = await _fileSystem.CreateTempFileAsync("content", ".html");

        Assert.Equal(_testDir, tempFile.BaseDirectory);

        // Cleanup
        await _fileSystem.DeleteTempFolderAsync();
    }

    // --- GetFiles edge case: trailing separator ---

    [Fact]
    public void GetFiles_TrailingSlashOnBasePath_HandledCorrectly()
    {
        File.WriteAllText(Path.Combine(_testDir, "page.md"), "content");
        var pathWithSlash = _testDir + Path.DirectorySeparatorChar;

        var files = _fileSystem.GetFiles(pathWithSlash).ToList();

        Assert.Single(files);
        Assert.Equal(_testDir, files[0].BaseDirectory);
    }
}
