namespace IronBeard.Core.Features.Logging
{
    public interface ILogger
    {
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Fatal(string message);
        void Progress(int percent, string message);
    }
}