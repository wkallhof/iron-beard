using System;
using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace IronBeard.Cli.Features.Commands
{
    /// <summary>
    /// Main command handler. If no sub-command is passed in,
    /// it defaults the request to the `generate` command.
    /// </summary>
    [Command("beard", ThrowOnUnexpectedArgument = false)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand("generate", typeof(GenerateCommand))]
    [Subcommand("watch", typeof(WatchCommand))]
    public class BeardCommand
    {
        public string[] RemainingArgs { get; set; }

        /// <summary>
        /// If this execute method is hit, the user didn't pass in a subcommand.
        /// Default to `generate`
        /// </summary>
        /// <param name="app">App context</param>
        /// <returns>Status code</returns>
        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            return await CommandLineApplication.ExecuteAsync<GenerateCommand>(RemainingArgs);
        }

        /// <summary>
        /// Get's the current version of the CLI application by reading
        /// the assembly for the version info
        /// </summary>
        /// <returns>Version info</returns>
        private static string GetVersion() => typeof(BeardCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}