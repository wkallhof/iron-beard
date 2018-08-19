using System;
using System.IO;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;
using Microsoft.Extensions.Options;

namespace IronBeard.Core.Features.Routing
{
    /// <summary>
    /// Manages building a file's URL
    /// </summary>
    public interface IUrlProvider
    {
        /// <summary>
        /// Get's the URL path for the given InputFile
        /// </summary>
        /// <param name="file">File to build URL from</param>
        /// <returns>URL of file</returns>
        string GetUrl(InputFile file);
    }

    /// <summary>
    /// Implementation of IUrlProvider that
    /// uses a file's extensions and config settings
    /// to return a proper URL for the file to be rendered in the site
    /// 
    /// TODO: Consider how to use this with OutputFile instead
    /// </summary>
    public class UrlProvider : IUrlProvider
    {
        private BeardConfig _config;

        public UrlProvider(BeardConfig config){
            this._config = config;
        }

        /// <summary>
        /// Given the InputFile, this scans the files extensions, and builds the correct
        /// URL path based on the file and current BeardConfig settings
        /// </summary>
        /// <param name="file">File to return URL for</param>
        /// <returns>File URL</returns>
        public string GetUrl(InputFile file){

            // for our Index files, we just return the directory path. The static servers
            // automatically look for the index.html file to render
            if(file.Name.IgnoreCaseEquals(this._config.IndexFileName))
                return file.RelativeDirectory;

            // For any other file, determine what the eventual extension will be.
            // If it is .md or .cshtml, return .HTML. Any other extension is copied as is
            var extension = file.Extension.IgnoreCaseEquals(".cshtml") 
                    || file.Extension.IgnoreCaseEquals(".md")
                    ?  ".html" : file.Extension;

            // check if we need to exclude HTML extensions. If so, exclude it from the URL as well
            extension = this._config.ExcludeHtmlExtension && extension.IgnoreCaseEquals(".html") ? string.Empty : extension;

            // return resulting URL
            return Path.Combine(file.RelativeDirectory, file.Name + extension);
        }
    }
}