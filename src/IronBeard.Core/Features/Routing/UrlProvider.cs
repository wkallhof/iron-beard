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
    }

    /// <summary>
    /// TODO: Consider how to use this with OutputFile instead
    /// </summary>
    public class UrlProvider : IUrlProvider
    {
        private BeardConfig _config;

        public UrlProvider(BeardConfig config){
            this._config = config;
        }

        public string GetUrl(InputFile file){
            if(file.Name.IgnoreCaseEquals(this._config.IndexFileName))
                return file.RelativeDirectory;

            var extension = file.Extension.IgnoreCaseEquals(".cshtml") 
                    || file.Extension.IgnoreCaseEquals(".md")
                    ?  ".html" : file.Extension;

            extension = this._config.ExcludeHtmlExtension && extension.IgnoreCaseEquals(".html") ? string.Empty : extension;

            return Path.Combine(file.RelativeDirectory, file.Name + extension);
        }
    }
}