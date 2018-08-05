namespace IronBeard.Core.Features.FileSystem
{
    public class InputFile
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string FullDirectory { get; set; }
        public string FullPath { get; set; }
        public string RelativeDirectory { get; set; }
    }
}