using System;
using System.IO;
using AutoGladiators.Core.Services.Storage;

namespace AutoGladiators.Core.Services.Logging;

public sealed class FileSink : ILogSink
{
    private readonly IAppStorage? _storage;
    private static readonly Microsoft.Extensions.Logging.ILogger Log = (Microsoft.Extensions.Logging.ILogger)AppLog.For<FileSink>();

    private readonly string _dir;
    public LogLevel MinLevel { get; }
    public FileSink(string? directory = null, LogLevel min = LogLevel.Info)
    {
        _dir = directory ?? "logs";
        Directory.CreateDirectory(_dir);
        MinLevel = min;
    }
    public void Write(LogEvent e)
    {
        var path = Path.Combine(_dir, $"log-{DateTime.UtcNow:yyyyMMdd}.txt");
        var line = $"{e.Timestamp:u}\t{e.Level}\t{e.Category}\t{e.Message}";
        try { File.AppendAllText(path, line + Environment.NewLine); } catch { /* swallow */ }
    }
}
