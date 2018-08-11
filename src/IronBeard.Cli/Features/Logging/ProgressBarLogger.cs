using IronBeard.Core.Features.Logging;
using Colorful;
using System.Drawing;

namespace IronBeard.Cli.Features.Logging
{
    public class ProgressBarLogger : ILogger
    {
        public void Error(string message)
        {
            Console.WriteLine(message, Color.Red);
        }

        public void Fatal(string message)
        {
            Console.WriteLine(message, Color.Red);
        }

        public void Info(string message)
        {
            Console.WriteLine(message, Color.Green);
        }

        public void Progress(int percent, string message)
        {
            Console.WriteLine(message, Color.Green);
        }

        public void Warn(string message)
        {
            Console.WriteLine(message, Color.Yellow);
        }
    }
}