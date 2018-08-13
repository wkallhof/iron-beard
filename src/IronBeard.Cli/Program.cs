

using System;
using System.Threading.Tasks;
using IronBeard.Cli.Features.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace IronBeard.Cli
{
    class Program
    {
        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<BeardCommand>(args);
    }
}
