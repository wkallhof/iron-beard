using System;
using System.Linq;
using System.Threading.Tasks;
using Raud.Core.Features.FileSystem;
using Raud.Core.Extensions;
using System.IO;

namespace Raud.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.WriteLine("Arguments : " + string.Join(", ", args));
            Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");

            var path = args.FirstOrDefault() ?? string.Empty;
            var directory =  Path.Join(Environment.CurrentDirectory, path);

            var fileSystem = new DiskFileSystem();
            var result = fileSystem.GetFiles(directory);
            if(!result.Success){
                Console.WriteLine($"Error: {result.Error}");
                return;
            }

            result.Data.ToList().ForEach(x => Console.WriteLine($"Name: {x.Name}, Extension: {x.Extension}, Directory: {x.Directory}"));


        }

    }
}
