namespace IronBeard.Core.Features.Logging;

public interface ILogger
{
    void Ascii(string message);
    void Info<T>(string message);
    void Warn<T>(string message);
    void Error<T>(string message);
    void Fatal<T>(string message);
}
