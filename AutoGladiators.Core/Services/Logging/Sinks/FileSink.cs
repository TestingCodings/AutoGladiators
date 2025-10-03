using System;
using System.IO;
using AutoGladiators.Core.Services.Storage;

namespace AutoGladiators.Core.Services.Logging;

public sealed class FileSink : ILogSink
{
    private static readonly IAppLogger Log = AppLog.For<FileSink>();

    private readonly string _dir;
    public LogLevel MinLevel { get; }
    
    public FileSink(string? directory = null, LogLevel min = LogLevel.Info)
    {
        // Use platform-appropriate directory for Android access
        _dir = GetPlatformLogDirectory(directory);
        
        try
        {
            Directory.CreateDirectory(_dir);
            Log.Info($"Log directory created at: {_dir}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to create log directory: {ex.Message}", ex);
        }
        
        MinLevel = min;
    }
    
    private static string GetPlatformLogDirectory(string? directory)
    {
        if (!string.IsNullOrEmpty(directory))
            return directory;
            
#if ANDROID
        try
        {
            // Try to use external storage directory accessible via file manager
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!string.IsNullOrEmpty(documentsPath))
            {
                return Path.Combine(documentsPath, "AutoGladiators", "logs");
            }
            
            // Fallback to a simple external path
            return Path.Combine("/sdcard", "AutoGladiators", "logs");
        }
        catch
        {
            // Final fallback to app directory
            return Path.Combine(Path.GetTempPath(), "AutoGladiators", "logs");
        }
#else
        return "logs";
#endif
    }
    
    public void Write(LogEvent e)
    {
        try
        {
            var path = Path.Combine(_dir, $"autogladiators-{DateTime.UtcNow:yyyyMMdd}.log");
            var line = $"{e.Timestamp:yyyy-MM-dd HH:mm:ss.fff}\t[{e.Level}]\t{e.Category}\t{e.Message}";
            
            // Ensure directory exists before writing
            Directory.CreateDirectory(_dir);
            
            File.AppendAllText(path, line + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Log to system console as fallback
            System.Diagnostics.Debug.WriteLine($"FileSink write failed: {ex.Message}");
        }
    }
}
