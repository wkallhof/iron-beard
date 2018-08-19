

using System;
using System.Threading.Tasks;
using IronBeard.Cli.Features.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace IronBeard.Cli
{
    class Program
    {
        /// <summary>
        /// Program entry, pass execution to our main command handler
        /// </summary>
        /// <param name="args">Args to pass to handler</param>
        /// <typeparam name="BeardCommand">Command handler</typeparam>
        /// <returns>Status Code 0 = success, 1 = failure</returns>
        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<BeardCommand>(args);
    }
}
