namespace AutoGladiators.Client.Services.Logging;
public interface ILogSink
{
    void Write(LogEvent e);
    LogLevel MinLevel { get; }
}
