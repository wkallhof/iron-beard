using System;
using System.IO;
using System.Threading.Tasks;
using IronBeard.Cli.Features.Logging;
using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using IronBeard.Core.Features.Formatting;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using IronBeard.Core.Features.Markdown;
using IronBeard.Core.Features.Razor;
using IronBeard.Core.Features.Routing;
using IronBeard.Core.Features.Static;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using EntryCli = EntryPoint.Cli;

namespace IronBeard.Cli.Features.Generate
{
    public class GenerateCommandHandler
    {
        public async Task Handle(string[] args){

            // process arguments
            var arguments = EntryCli.Parse<GenerateCommandArguments>(args);
            
            // normalize inputs
            var inputArg = arguments.InputDirectory;
            var outputArg = arguments.OutputDirectory ?? Path.Combine(inputArg, "www");

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
                await generator.Generate();
            }
            catch(Exception e){
                logger.Fatal(e.Message);
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
            services.AddSingleton<ILogger, ProgressBarLogger>();
            services.AddSingleton<IFileSystem, DiskFileSystem>();
            services.AddTransient<IUrlProvider, UrlProvider>();
            services.AddSingleton<RazorViewRenderer>();
            services.AddSingleton<StaticGenerator>();

            return services.BuildServiceProvider();
        }
    }
}