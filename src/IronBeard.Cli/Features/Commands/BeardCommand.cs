using System;
using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace IronBeard.Cli.Features.Commands
{
    [Command("beard", ThrowOnUnexpectedArgument = false)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand("generate", typeof(GenerateCommand))]
    [Subcommand("watch", typeof(WatchCommand))]
    [Subcommand("server", typeof(ServerCommand))]
    [Subcommand("init", typeof(InitCommand))]
    public class BeardCommand
    {
        public string[] RemainingArgs { get; set; }
        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            return await CommandLineApplication.ExecuteAsync<GenerateCommand>(RemainingArgs);
        }

        private static string GetVersion() => typeof(BeardCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}