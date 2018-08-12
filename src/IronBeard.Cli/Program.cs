

using System;
using System.Threading.Tasks;
using IronBeard.Cli.Features.Commands;
using EntryCli = EntryPoint.Cli;

namespace IronBeard.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            EntryCli.Execute<BeardCommands>(args);
            Console.WriteLine("Exiting");
        }
    }
}
