using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using IronBeard.Core.Features.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.PlatformAbstractions;

namespace IronBeard.Core.Features.Razor
{
    public class RazorViewRenderer
    {
        private RazorViewToStringRenderer _renderer;
        private string _inputDirectory;

        public RazorViewRenderer(string inputDirectory){
            this._inputDirectory = inputDirectory;
            this.Setup();
        }

        public async Task<string> RenderAsync<T>(string viewPath, T model ){
            return await this._renderer.RenderViewToStringAsync(viewPath, model);
        }

        public void Setup(){

            var services = new ServiceCollection();
            var applicationEnvironment = PlatformServices.Default.Application;
            services.AddSingleton(applicationEnvironment);

            var environment = new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly().GetName().Name
            };
            services.AddSingleton<IHostingEnvironment>(environment);

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