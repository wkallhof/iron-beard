using System;
using System.Threading.Tasks;
using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.Generator;
using IronBeard.Cli.Features.Logging;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Formatting;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Markdown;
using IronBeard.Core.Features.Razor;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Static;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace IronBeard.Cli.Features.Commands
{
    [Command(Description = "Generates a static site from the files in the given directory", ThrowOnUnexpectedArgument = false)]
    public class GenerateCommand
    {
        [Option("-i|--input <PATH>", "Provide the root directory where Iron Beard should look for files to generate a static site from.", CommandOptionType.SingleValue)]
        [DirectoryExists]
        public string InputDirectory { get; set; } = ".";

        [Option("-o|--output <PATH>", "Provide the directory where Iron Beard should write the static site to.", CommandOptionType.SingleValue)]
        public string OutputDirectory { get; set; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            // normalize inputs
            var inputArg = InputDirectory;
            var outputArg = OutputDirectory ?? Path.Combine(inputArg, "www");

            var inputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, inputArg));
            var outputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputArg));

            // configure services
            var services = ConfigureServices(inputPath, outputPath);

            var logger = services.GetService<ILogger>();

            var generator = services.GetService<StaticGenerator>();

            generator.AddProcessor(services.GetService<MarkdownProcessor>());
            generator.AddProcessor(services.GetService<RazorProcessor>());
            generator.AddProcessor(services.GetService<StaticProcessor>());
            generator.AddProcessor(services.GetService<HtmlFormatProcessor>());

            try{
                logger.Ascii("Iron Beard");
                await generator.Generate();
                return 0;
            }
            catch(Exception e){
                logger.Fatal<GenerateCommand>(e.Message);
                return 1;
            }
        }

        private ServiceProvider ConfigureServices(string inputDirectory, string outputDirectory)
        {
            var services = new ServiceCollection();

            // build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(inputDirectory)
                .AddJsonFile("beard.json")
                .Build();

            services.AddOptions();

            var config = new BeardConfig();
            configuration.Bind("Config", config);
            services.AddSingleton(config);

            services.AddSingleton<GeneratorContext>(new GeneratorContext(inputDirectory, outputDirectory));
            services.AddSingleton<MarkdownProcessor>();
            services.AddSingleton<RazorProcessor>();
            services.AddSingleton<StaticProcessor>();
            services.AddSingleton<HtmlFormatProcessor>();
            services.AddSingleton<ILogger, ConsoleLogger>();
            services.AddSingleton<IFileSystem, DiskFileSystem>();
            services.AddTransient<IUrlProvider, UrlProvider>();
            services.AddSingleton<RazorViewRenderer>();
            services.AddSingleton<StaticGenerator>();

            return services.BuildServiceProvider();
        }
    }
}