using System;
using System.Threading.Tasks;
using EntryPoint;
using IronBeard.Cli.Features.Generate;
using EntryCli = EntryPoint.Cli;

namespace IronBeard.Cli.Features.Commands
{
    public class BeardCommands : BaseCliCommands
    {
        [DefaultCommand]
        [Command("generate")]
        [Help("Generates a static site from the files in the given directory")]
        public void Generate(string[] args) {
            var handler = new GenerateCommandHandler();
            Task.WaitAll(handler.Handle(args));
        }

        [Command("server")]
        [Help("Starts a lightweight webserver for your static site")]
        public void Secondary(string[] args) {
            Console.WriteLine("Not supported yet.");
        }

        [Command("init")]
        [Help("Scaffolds a new static site for you.")]
        public void Init(string[] args) {
            Console.WriteLine("Not supported yet.");
        }
    }
}