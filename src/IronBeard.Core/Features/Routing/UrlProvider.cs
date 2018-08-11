using System;
using System.IO;
using IronBeard.Core.Extensions;
using IronBeard.Core.Features.Configuration;
using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Features.Routing
{
    public static class UrlProvider
    {
        public static string GetUrl(InputFile file){
            if(file.Name.IgnoreCaseEquals(Config.IndexFileName))
                return file.RelativeDirectory;

            return Path.Combine(file.RelativeDirectory, file.Name);
        }

        public static string GetUrlWithExtension(InputFile file)
        {
            return Path.Combine(file.RelativeDirectory, file.Name + file.Extension);
        }
    }
}