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
using System.Reflection;

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

        /// <summary>
        /// Main execution method for the Generate command. Normalizes the inputs,
        /// builds up the DI container, adds processors, and executes generator
        /// </summary>
        /// <param name="app">App context</param>
        /// <returns>Status code</returns>
        public async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            // normalize inputs
            var inputArg = InputDirectory;
            var outputArg = OutputDirectory ?? Path.Combine(inputArg, "www");

            var inputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, inputArg));
            var outputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, outputArg));

            // configure services
            var services = ConfigureServices(inputPath, outputPath);

            // fetch the services we need right now
            var logger = services.GetService<ILogger>();
            var generator = services.GetService<StaticGenerator>();

            // add our processors in the desired order
            generator.AddProcessor(services.GetService<MarkdownProcessor>());
            generator.AddProcessor(services.GetService<RazorProcessor>());
            generator.AddProcessor(services.GetService<StaticProcessor>());
            generator.AddProcessor(services.GetService<HtmlFormatProcessor>());

            try{
                var startTime = DateTime.Now;
                logger.Info<GenerateCommand>($"--- Iron Beard v{this.GetVersion()} --- ");
                await generator.Generate();
                var completeTime = DateTime.Now;
                logger.Info<GenerateCommand>($"Completed in {(completeTime - startTime).TotalSeconds.ToString("N2")}s");
                return 0;
            }
            catch(Exception e){
                logger.Fatal<GenerateCommand>(e.Message);
                return 1;
            }
        }

        /// <summary>
        /// Builds up our service container for DI
        /// </summary>
        /// <param name="inputDirectory">Defined Input Directory</param>
        /// <param name="outputDirectory">Defined Output Directory</param>
        /// <returns>Service Provider</returns>
        private ServiceProvider ConfigureServices(string inputDirectory, string outputDirectory)
        {
            var services = new ServiceCollection();

            // build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(inputDirectory)
                .AddJsonFile("beard.json", optional:true)
                .Build();

            services.AddOptions();

            // bind to our config model
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

        /// <summary>
        /// Get's the current version of the CLI application by reading
        /// the assembly for the version info
        /// </summary>
        /// <returns>Version info</returns>
        private string GetVersion() => typeof(BeardCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}