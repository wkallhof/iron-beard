using System;
using System.Linq;
using System.Threading.Tasks;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Extensions;
using System.IO;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Markdown;
using IronBeard.Core.Features.Razor;
using IronBeard.Core.Features.Static;
using System.Collections.Generic;

namespace IronBeard.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processor = new CommandProcessor();
            await processor.Process(args);
        }


    }


}
