namespace Raud.Core.Features.FileSystem
{
    public class OutputFile : InputFile
    {
        public string Content { get; set; }
        public InputFile Input { get; set; }
        public bool DirectCopy { get; set; }

        public static OutputFile FromInputFile(InputFile file){
            return new OutputFile() { 
                Name = file.Name, 
                Extension = file.Extension, 
                FullDirectory = file.FullDirectory,
                FullPath = file.FullPath,
                RelativeDirectory = file.RelativeDirectory,
                Input = file
            };
        }
    }
}