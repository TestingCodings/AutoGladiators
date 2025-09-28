namespace AutoGladiators.Core.Services.Logging;
public interface ILogSink
{
    void Write(LogEvent e);
    LogLevel MinLevel { get; }
}
