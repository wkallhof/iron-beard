using System.Drawing;
using System.Threading.Tasks;
using Colorful;
using McMaster.Extensions.CommandLineUtils;

namespace IronBeard.Cli.Features.Commands
{
    [Command(Description = "Scaffolds a new static site for you.")]
    public class InitCommand
    {
        public int OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Not supported yet.", Color.Red);
            return 1;
        }
    }
}