using AutoGladiators.Client.Services.Logging;
using Microsoft.Extensions.Logging;
// Services/Logging/LogManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace AutoGladiators.Client.Services.Logging;

public sealed class LogManager
{
    private static readonly IAppLogger Log = AppLog.For<LogManager>();

    public static LogManager Instance { get; } = new LogManager();
    private readonly object _gate = new();
    private readonly List<ILogSink> _sinks = new();

    private LogManager() { }

    public void AddSink(ILogSink sink)
    {
        lock (_gate) _sinks.Add(sink);
    }

    public IAppLogger ForCategory(string category) => new Logger(this, category);

    internal void Write(LogEvent e)
    {
        ILogSink[] sinks;
        lock (_gate) sinks = _sinks.ToArray();
        foreach (var s in sinks)
        {
            if (e.Level >= s.MinLevel)
            {
                try { s.Write(e); } catch { /* swallow */ }
            }
        }
    }

    private sealed class Logger : IAppLogger
    {
        private readonly LogManager _mgr;
        private readonly string _cat;
        public Logger(LogManager mgr, string category) { _mgr = mgr; _cat = category; }

        public void Log(LogLevel level, string message, Exception? ex = null, string? correlationId = null, IReadOnlyDictionary<string, object?>? props = null)
            => _mgr.Write(new LogEvent(DateTimeOffset.Now, level, _cat, message, ex, correlationId, props));

        public void Trace(string m, IReadOnlyDictionary<string, object?>? p = null) => Log(LogLevel.Trace, m, null, null, p);
        public void Debug(string m, IReadOnlyDictionary<string, object?>? p = null) => Log(LogLevel.Debug, m, null, null, p);
        public void Info(string m, IReadOnlyDictionary<string, object?>? p = null)  => Log(LogLevel.Info,  m, null, null, p);
        public void Warn(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) => Log(LogLevel.Warn, m, ex, null, p);
        public void Error(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null)=> Log(LogLevel.Error, m, ex, null, p);
        public void Critical(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null)=> Log(LogLevel.Critical, m, ex, null, p);
    }
}
