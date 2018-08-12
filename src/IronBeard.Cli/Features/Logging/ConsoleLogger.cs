using IronBeard.Core.Features.Logging;
using IronBeard.Core.Extensions;
using Colorful;
using System.Drawing;

namespace IronBeard.Cli.Features.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Error<T>(string message) => WriteLine<T>(message, Color.Red);
        public void Fatal<T>(string message) => WriteLine<T>(message, Color.Red);
        public void Info<T>(string message) => WriteLine<T>(message, Color.Green);
        public void Warn<T>(string message) => WriteLine<T>(message, Color.Yellow);

        public void Ascii(string message){
            Console.WriteAscii(message,Color.White);
        }

        private void WriteLine<T>(string message, Color color)
        {
            Console.Write("[", Color.Green);
            Console.Write($"{typeof(T).Name}", Color.Yellow);
            Console.Write("] ", Color.Green);

            Console.Write( $"{message}\n", color);
        }
    }
}