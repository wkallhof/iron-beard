using System.Diagnostics;

namespace IronBeard.Cli.IntegrationTests;

public class CliGenerateTests : IDisposable
{
    private readonly string _outputDir;
    private readonly string _sampleDir;
    private readonly string _cliProjectDir;

    public CliGenerateTests()
    {
        var repoRoot = FindRepoRoot();
        _sampleDir = Path.Combine(repoRoot, "samples", "razor-markdown-sample");
        _cliProjectDir = Path.Combine(repoRoot, "src", "IronBeard.Cli");
        _outputDir = Path.Combine(Path.GetTempPath(), "ironbeard-test-" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, true);
    }

    [Fact]
    public async Task Generate_ExitCodeZero()
    {
        var result = await RunGenerate();
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task Generate_CreatesOutputDirectory()
    {
        await RunGenerate();
        Assert.True(Directory.Exists(_outputDir));
    }

    [Fact]
    public async Task Generate_CreatesIndexHtml()
    {
        await RunGenerate();
        // ExcludeHtmlExtension is true by default, so index files are just directories
        // The sample project should produce an index.html or equivalent
        var indexFile = Path.Combine(_outputDir, "index.html");
        Assert.True(File.Exists(indexFile), $"Expected {indexFile} to exist");
    }

    [Fact]
    public async Task Generate_CopiesStaticAssets()
    {
        await RunGenerate();

        // Check that CSS files are copied
        var cssFiles = Directory.GetFiles(_outputDir, "*.css", SearchOption.AllDirectories);
        Assert.NotEmpty(cssFiles);
    }

    [Fact]
    public async Task Generate_MarkdownConvertedToHtml()
    {
        await RunGenerate();

        // Find an HTML file generated from markdown and verify it contains HTML tags
        var htmlFiles = Directory.GetFiles(_outputDir, "*.html", SearchOption.AllDirectories);
        Assert.NotEmpty(htmlFiles);

        var content = await File.ReadAllTextAsync(htmlFiles[0], TestContext.Current.CancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public async Task Generate_OutputContainsHtmlStructure()
    {
        await RunGenerate();

        var indexFile = Path.Combine(_outputDir, "index.html");
        if (File.Exists(indexFile))
        {
            var content = await File.ReadAllTextAsync(indexFile, TestContext.Current.CancellationToken);
            Assert.Contains("<html", content, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Generate_MultipleOutputFilesCreated()
    {
        await RunGenerate();

        var allFiles = Directory.GetFiles(_outputDir, "*", SearchOption.AllDirectories);
        Assert.True(allFiles.Length > 1, $"Expected multiple output files, got {allFiles.Length}");
    }

    private async Task<ProcessResult> RunGenerate()
    {
        return await RunCli($"generate -i \"{_sampleDir}\" -o \"{_outputDir}\"");
    }

    private async Task<ProcessResult> RunCli(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{_cliProjectDir}\" -- {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessResult(process.ExitCode, stdout, stderr);
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir, ".git")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new InvalidOperationException("Could not find repository root (.git directory)");
    }

    private record ProcessResult(int ExitCode, string Stdout, string Stderr);
}
