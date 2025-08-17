using System.Collections.Generic;
namespace AutoGladiators.Client.Services.Logging;
public interface IAppLogger
{
    void Log(LogLevel level, string message, Exception? ex = null, string? correlationId = null, IReadOnlyDictionary<string, object?>? props = null);
    void Trace(string m, IReadOnlyDictionary<string, object?>? p = null);
    void Debug(string m, IReadOnlyDictionary<string, object?>? p = null);
    void Info(string m, IReadOnlyDictionary<string, object?>? p = null);
    void Warn(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null);
    void Error(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null);
    void Critical(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null);
}
