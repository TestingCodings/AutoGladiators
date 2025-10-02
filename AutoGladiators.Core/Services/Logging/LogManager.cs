using System;
using System.Collections.Generic;
using AutoGladiators.Core.Services.Logging;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Core.Services.Logging;

public sealed class LogManager
{
    // If you want a logger for LogManager itself, use IAppLogger (not MS ILogger)
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

        public void Trace(string m, IReadOnlyDictionary<string, object?>? p = null) =>
            Log(LogLevel.Trace, m, null, null, p);

        public void Debug(string m, IReadOnlyDictionary<string, object?>? p = null) =>
            Log(LogLevel.Debug, m, null, null, p);

        public void Info(string m, IReadOnlyDictionary<string, object?>? p = null)  =>
            Log(LogLevel.Info,  m, null, null, p);

        public void Warn(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) =>
            Log(LogLevel.Warn, m, ex, null, p);

        public void Error(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) =>
            Log(LogLevel.Error, m, ex, null, p);

        public void Critical(string m, Exception? ex = null, IReadOnlyDictionary<string, object?>? p = null) =>
            Log(LogLevel.Error, m, ex, null, p); // Use Error if Critical is not defined in your custom LogLevel

        public void LogPerformance(string operation, long durationMs, IReadOnlyDictionary<string, object?>? properties = null)
        {
            var props = new Dictionary<string, object?> { ["Operation"] = operation, ["DurationMs"] = durationMs };
            if (properties != null)
            {
                foreach (var kvp in properties)
                    props[kvp.Key] = kvp.Value;
            }
            Log(LogLevel.Info, $"Performance: {operation} took {durationMs}ms", null, null, props);
        }

        public void LogBattleEvent(string eventType, string playerId, string enemyId, IReadOnlyDictionary<string, object?>? properties = null)
        {
            var props = new Dictionary<string, object?> { ["EventType"] = eventType, ["PlayerId"] = playerId, ["EnemyId"] = enemyId };
            if (properties != null)
            {
                foreach (var kvp in properties)
                    props[kvp.Key] = kvp.Value;
            }
            Log(LogLevel.Info, $"Battle Event: {eventType}", null, null, props);
        }

        public void LogUserAction(string action, string context, IReadOnlyDictionary<string, object?>? properties = null)
        {
            var props = new Dictionary<string, object?> { ["Action"] = action, ["Context"] = context };
            if (properties != null)
            {
                foreach (var kvp in properties)
                    props[kvp.Key] = kvp.Value;
            }
            Log(LogLevel.Info, $"User Action: {action}", null, null, props);
        }

        public void LogGameState(string state, string context, IReadOnlyDictionary<string, object?>? properties = null)
        {
            var props = new Dictionary<string, object?> { ["State"] = state, ["Context"] = context };
            if (properties != null)
            {
                foreach (var kvp in properties)
                    props[kvp.Key] = kvp.Value;
            }
            Log(LogLevel.Info, $"Game State: {state}", null, null, props);
        }

        public void LogError(string message, string context, Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null)
        {
            var props = new Dictionary<string, object?> { ["Context"] = context };
            if (properties != null)
            {
                foreach (var kvp in properties)
                    props[kvp.Key] = kvp.Value;
            }
            Log(LogLevel.Error, message, exception, null, props);
        }
    }
}
