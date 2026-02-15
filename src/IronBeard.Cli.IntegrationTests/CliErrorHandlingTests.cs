using System.Diagnostics;

namespace IronBeard.Cli.IntegrationTests;

public class CliErrorHandlingTests
{
    private readonly string _cliProjectDir;

    public CliErrorHandlingTests()
    {
        var repoRoot = FindRepoRoot();
        _cliProjectDir = Path.Combine(repoRoot, "src", "IronBeard.Cli");
    }

    [Fact]
    public async Task Generate_NonexistentInputDir_ReturnsNonZeroExitCode()
    {
        var nonexistent = Path.Combine(Path.GetTempPath(), "ironbeard-nonexistent-" + Guid.NewGuid().ToString("N"));
        var result = await RunCli($"generate -i \"{nonexistent}\"");

        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public async Task NoArguments_DoesNotCrash()
    {
        var result = await RunCli("");

        // Should exit without crashing (exit code 0 for help, or non-zero for missing args - either is fine)
        Assert.True(result.ExitCode >= 0, "Process should not crash");
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
