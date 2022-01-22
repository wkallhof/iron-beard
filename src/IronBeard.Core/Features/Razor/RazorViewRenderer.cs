using System.Diagnostics;
using System.Reflection;
using IronBeard.Core.Features.Generator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.PlatformAbstractions;

namespace IronBeard.Core.Features.Razor
{
    /// <summary>
    /// Razor View renderer that leverages the AspNetCore MVC razor engine
    /// to render Razor templates to a string
    /// </summary>
    public class RazorViewRenderer
    {
        private readonly RazorViewToStringRenderer _renderer;
        private readonly string _inputDirectory;

        public RazorViewRenderer(GeneratorContext context){
            _inputDirectory = context.InputDirectory;
            
            /// <summary>
            /// Sets up the RazorViewToStringRenderer. Unfortunately it appears that the 
            /// Razor View engine is tightly coupled with AspNet MVC. We need to build up a DI
            /// Service container so we can get the right context for the RazorView engine
            /// to render files.
            /// </summary>
            var services = new ServiceCollection();
            var applicationEnvironment = PlatformServices.Default.Application;
            services.AddSingleton(applicationEnvironment);

            var environment = new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name
            };
            services.AddSingleton<IHostingEnvironment>(environment);

            // sets up the context of the renderer to our input directory. Paths
            // to views are relative to this directory
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(new PhysicalFileProvider(_inputDirectory));
            });

            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<DiagnosticSource>(diagnosticSource);

            services.AddLogging();
            services.AddMvc();
            services.AddSingleton<RazorViewToStringRenderer>();
            var provider = services.BuildServiceProvider();
            _renderer = provider.GetRequiredService<RazorViewToStringRenderer>();
        }

        /// <summary>
        /// Renders the file at the given view path using the given model
        /// </summary>
        /// <param name="viewPath">Path to view to render</param>
        /// <param name="model">Model to pass into view for rendering</param>
        /// <typeparam name="T">Type of model</typeparam>
        /// <returns>String of rendered content</returns>
        public async Task<string> RenderAsync<T>(string viewPath, T model ){
            return await _renderer.RenderViewToStringAsync(viewPath, model);
        }
    }
}