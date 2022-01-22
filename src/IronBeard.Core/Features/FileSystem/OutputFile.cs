namespace IronBeard.Core.Features.FileSystem
{
    public record OutputFile
    {
        public InputFile Input { get; private set;}
        public string Name { get; private set;}
        public string Extension { get; set;}
        public string BaseDirectory { get; private set;}
        public string RelativeDirectory { get; private set;}

        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        public bool DirectCopy { get; set; }
        public string? Content { get; set; }
        public string? Url { get; set; }
        public string FullDirectory => Path.GetFullPath(BaseDirectory + RelativeDirectory);
        public string FullPath => Path.Combine(FullDirectory, Name + Extension);
        public string RelativePath => Path.Combine(RelativeDirectory, Name + Extension);

        public OutputFile(InputFile inputFile, string baseDirectory)
        {
            Input = inputFile;
            Name = inputFile.Name;
            Extension = inputFile.Extension;
            BaseDirectory = baseDirectory;
            RelativeDirectory = inputFile.RelativeDirectory;
        }

    }
}