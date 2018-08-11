
using System.Collections.Generic;
using System.Linq;
using IronBeard.Core.Features.FileSystem;

namespace IronBeard.Core.Features.Generator
{
    public class ViewContext
    {
        public OutputFile Current {get;set;}

        public IEnumerable<OutputFile> Siblings { get; set; }
        public IEnumerable<OutputFile> Children { get; set; }
        public IEnumerable<OutputFile> All { get; set; }

        public ViewContext(OutputFile current, GeneratorContext context){
            this.Current = current;

            this.Siblings = context.OutputFiles.Where(x => x.RelativeDirectory.Equals(current.RelativeDirectory) && x != current);
            this.Children = context.OutputFiles.Where(x => x.RelativeDirectory.Contains(current.RelativeDirectory) && !x.RelativeDirectory.Equals(current.RelativeDirectory));
            this.All = context.OutputFiles;
        }
    }
}