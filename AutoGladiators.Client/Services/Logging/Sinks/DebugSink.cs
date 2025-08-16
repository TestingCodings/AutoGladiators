using System.Diagnostics;
namespace AutoGladiators.Client.Services.Logging;
public sealed class DebugSink : ILogSink
{
    private static readonly IAppLogger Log = AppLog.For<DebugSink>();

    public LogLevel MinLevel { get; }
    public DebugSink(LogLevel min = LogLevel.Debug) => MinLevel = min;
    public void Write(LogEvent e)
        => Debug.WriteLine($"{e.Timestamp:HH:mm:ss.fff} [{e.Level}] {e.Category} :: {e.Message}");
}
