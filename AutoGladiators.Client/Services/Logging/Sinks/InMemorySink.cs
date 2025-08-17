using System.Collections.Generic;
namespace AutoGladiators.Client.Services.Logging;
public sealed class InMemorySink : ILogSink
{
    private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<InMemorySink>();

    private readonly int _capacity;
    private readonly object _gate = new();
    private readonly Queue<LogEvent> _q;
    public LogLevel MinLevel { get; }

    public InMemorySink(int capacity = 500, LogLevel min = LogLevel.Debug)
    { _capacity = capacity; MinLevel = min; _q = new Queue<LogEvent>(capacity); }

    public void Write(LogEvent e)
    {
        lock (_gate)
        {
            if (_q.Count >= _capacity) _q.Dequeue();
            _q.Enqueue(e);
        }
    }

    public LogEvent[] Snapshot()
    {
        lock (_gate) return _q.ToArray();
    }

    public void Clear()
    {
        lock (_gate) _q.Clear();
    }
}
