using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace IronBeard.Cli.Features.Commands
{
    [Command(Description = "Watch a directory for changes and rebuild automatically")]
    public class WatchCommand : GenerateCommand
    {
        private CommandLineApplication _app;
        private FileSystemWatcher _watcher;

        public new async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            this._app = app;
            
            var inputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, InputDirectory));

            this._watcher = new FileSystemWatcher(inputPath);
            this._watcher.Renamed += async (s, e) => await Renamed(s, e);
            this._watcher.Deleted += async (s, e) => await Changed(s, e);
            this._watcher.Changed += async (s, e) => await Changed(s, e);
            this._watcher.Created += async (s, e) => await Changed(s, e);
            this._watcher.IncludeSubdirectories = true;
            this._watcher.Filter = "";

            await this.RunGenerate();
            
            while (true) { };
        }

        private async Task Renamed(object sender, RenamedEventArgs e) {
            Console.WriteLine(DateTime.Now + ": " + e.ChangeType + " " + e.FullPath);
            await this.RunGenerate();
        }

        private async Task Changed(object sender, FileSystemEventArgs e) {
            Console.WriteLine(DateTime.Now + ": " + e.ChangeType + " " + e.FullPath);
            await this.RunGenerate();
        }

        private async Task RunGenerate(){
            this._watcher.EnableRaisingEvents = false;
            await base.OnExecuteAsync(this._app);
            this._watcher.EnableRaisingEvents = true;
            Console.WriteLine("Watching...");
        }
    }
}