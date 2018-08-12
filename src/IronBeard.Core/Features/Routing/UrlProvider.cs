using System;
using System.IO;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using Microsoft.Extensions.Options;

namespace IronBeard.Core.Features.Routing
{
    public interface IUrlProvider
    {
        string GetUrl(InputFile file);
        string GetUrlWithExtension(InputFile file);
    }

    public class UrlProvider : IUrlProvider
    {
        private BeardConfig _config;

        public UrlProvider(BeardConfig config){
            this._config = config;
        }

        public string GetUrl(InputFile file){
            if(file.Name.IgnoreCaseEquals(this._config.IndexFileName))
                return file.RelativeDirectory;

            return Path.Combine(file.RelativeDirectory, file.Name);
        }

        public string GetUrlWithExtension(InputFile file)
        {
            return Path.Combine(file.RelativeDirectory, file.Name + file.Extension);
        }
    }
}