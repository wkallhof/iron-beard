using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.Generator;
using IronBeard.Core.Features.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;

namespace IronBeard.Core.Features.Razor
{
    /// <summary>
    /// Razor View renderer that leverages the AspNetCore MVC razor engine
    /// to render Razor templates to a string
    /// </summary>
    public class RazorViewRenderer
    {
        private RazorViewToStringRenderer _renderer;
        private string _inputDirectory;

        public RazorViewRenderer(GeneratorContext context){
            this._inputDirectory = context.InputDirectory;
            this.Setup();
        }

        /// <summary>
        /// Renders the file at the given view path using the given model
        /// </summary>
        /// <param name="viewPath">Path to view to render</param>
        /// <param name="model">Model to pass into view for rendering</param>
        /// <typeparam name="T">Type of model</typeparam>
        /// <returns>String of rendered content</returns>
        public async Task<string> RenderAsync<T>(string viewPath, T model ){
            return await this._renderer.RenderViewToStringAsync(viewPath, model);
        }

        /// <summary>
        /// Sets up the RazorViewToStringRenderer. Unfortunately it appears that the 
        /// Razor View engine is tightly coupled with AspNet MVC. We need to build up a DI
        /// Service container so we can get the right context for the RazorView engine
        /// to render files.
        /// </summary>
        public void Setup(){

            var services = new ServiceCollection();
            var applicationEnvironment = PlatformServices.Default.Application;
            services.AddSingleton(applicationEnvironment);

            var environment = new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly().GetName().Name
            };
            services.AddSingleton<IHostingEnvironment>(environment);

            // sets up the context of the renderer to our input directory. Paths
            // to views are relative to this directory
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(new PhysicalFileProvider(this._inputDirectory));
            });

            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<DiagnosticSource>(diagnosticSource);

            services.AddLogging();
            services.AddMvc();
            services.AddSingleton<RazorViewToStringRenderer>();
            var provider = services.BuildServiceProvider();
            this._renderer = provider.GetRequiredService<RazorViewToStringRenderer>();
        }
    }
}