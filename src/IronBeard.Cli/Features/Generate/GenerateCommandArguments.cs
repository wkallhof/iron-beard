using EntryPoint;

namespace IronBeard.Cli.Features.Generate
{
    public class GenerateCommandArguments : BaseCliArguments
    {
        public GenerateCommandArguments() : base("Generate") { }

        [OptionParameter(ShortName: 'i', LongName: "input-dir")]
        [Help("Provide the root directory where Iron Beard should look for files to generate a static site from.")]
        public string InputDirectory { get; set; } = ".";

        [OptionParameter(ShortName: 'o', LongName: "output-dir")]
        [Help("Provide the directory where Iron Beard should write the static site to.")]
        public string OutputDirectory { get; set; }
    }
}