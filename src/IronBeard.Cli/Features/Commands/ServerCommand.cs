using System;
using System.Drawing;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace IronBeard.Cli.Features.Commands
{
    [Command(Description = "Starts a lightweight webserver for your static site")]
    public class ServerCommand
    {
        public int OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Not supported yet.", Color.Red);
            return 1;
        }
    }
}