using System;
using System.Drawing;
using System.Text;
using System.Threading;
using IronBeard.Core.Extensions;
using ColorConsole = Colorful.Console;

namespace IronBeard.Cli
{
    /// <summary>
    /// An ASCII progress bar
    /// </summary>
    public class ProgressBar : IDisposable
    {
        private const int BLOCK_COUNT = 20;
        private const string ANIMATION = @"|/-\";
        private readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        
        private readonly Timer _timer;

        private double _currentProgress = 0;

        private string _currentBarMessage = string.Empty;
        private string _currentBeforeMessage = string.Empty;
        private string _currentAfterMessage = string.Empty;

        private string _currentText = string.Empty;
        private bool _disposed = false;
        private int _animationIndex = 0;

        public ProgressBar()
        {
            this._timer = new Timer(this.TimerHandler);

            if (!ColorConsole.IsOutputRedirected)
                ResetTimer();
        }

        public void Report(double value, string message)
        {
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref this._currentProgress, value);
            Interlocked.Exchange(ref this._currentBarMessage, message);
        }

        public void MessageBefore(string message){
            Interlocked.Exchange(ref this._currentBeforeMessage, this._currentBeforeMessage + "\n"+ message);
        }

        public void MessageAfter(string message){
            Interlocked.Exchange(ref this._currentAfterMessage, message);
        }

        private void TimerHandler(object state)
        {
            lock (this._timer)
            {
                if (this._disposed)
                    return;

                var progressBlockCount = (int)(this._currentProgress * BLOCK_COUNT);
                var percent = (int)(this._currentProgress * 100);
                var text = string.Format("|{0}{1}| {2,3}%",
                    new string('█', progressBlockCount), new string('░', BLOCK_COUNT - progressBlockCount), percent);

                if(percent != 100)
                    text += $" {ANIMATION[this._animationIndex++ % ANIMATION.Length]} : {this._currentBarMessage}";

                var color = percent == 100 ? Color.Green : Color.Yellow;

                UpdateText(text, color);
                ResetTimer();
            }
        }

        private void UpdateText(string text, Color color)
        {
            // Get length of common portion
            var commonPrefixLength = 0;
            var commonLength = Math.Min(this._currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == this._currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            if(this._currentBeforeMessage.IsSet())
                commonPrefixLength = 0;

            // Backtrack to the first differing character
            var outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', this._currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            var overlapCount = this._currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            if(this._currentBeforeMessage.IsSet()){
                ColorConsole.WriteLine(this._currentBeforeMessage, Color.Green);
                this._currentBeforeMessage = string.Empty;
            }

            ColorConsole.Write(outputBuilder, color);
            this._currentText = text;
        }

        private void ResetTimer()
        {
            this._timer.Change(this._animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (this._timer)
            {
                this._currentProgress = 1;
                this._currentBarMessage = string.Empty;
                this.TimerHandler(null);
                this._disposed = true;
                Console.WriteLine();
            }
        }
    }
}