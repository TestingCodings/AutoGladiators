using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

// Helper class for logger scope
internal sealed class NoOpDisposable : IDisposable
{
    public static NoOpDisposable Instance { get; } = new();
    public void Dispose() { }
}

namespace AutoGladiators.Client.Services.Logging
{
    public sealed class AndroidFileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public AndroidFileLoggerProvider()
        {
            // Use public accessible directory - no permissions needed
            var publicDir = "/sdcard/AutoGladiators/Logs";
            
            // Try alternative accessible paths if primary fails
            var alternativePaths = new[]
            {
                "/storage/emulated/0/AutoGladiators/Logs",
                "/mnt/sdcard/AutoGladiators/Logs", 
                Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath + "/AutoGladiators/Logs",
                Platform.CurrentActivity?.GetExternalFilesDir(null)?.AbsolutePath + "/Logs"
            };

            _logDirectory = publicDir;
            
            // Try each path until one works
            foreach (var path in alternativePaths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                        _logDirectory = path;
                        break;
                    }
                    catch
                    {
                        // Try next path
                        continue;
                    }
                }
            }
            
            try
            {
                Directory.CreateDirectory(_logDirectory);
                
                // Write a readme file explaining how to access logs
                var readmePath = Path.Combine(_logDirectory, "README.txt");
                File.WriteAllText(readmePath, 
                    "AutoGladiators Log Files\n" +
                    "========================\n" +
                    $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                    $"Directory: {_logDirectory}\n\n" +
                    "Access via ADB:\n" +
                    "adb shell\n" +
                    "cd /sdcard/AutoGladiators/Logs\n" +
                    "ls -la\n" +
                    "cat app_YYYY-MM-DD.log\n\n" +
                    "Pull to PC:\n" +
                    "adb pull /sdcard/AutoGladiators/Logs .\n\n" +
                    "Or use Android Studio Device File Explorer:\n" +
                    "View > Tool Windows > Device File Explorer\n" +
                    "Navigate to sdcard/AutoGladiators/Logs\n");
                    
                System.Diagnostics.Debug.WriteLine($"AndroidFileLoggerProvider: Log directory created at {_logDirectory}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidFileLoggerProvider: Failed to create log directory: {ex.Message}");
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new AndroidFileLogger(categoryName, _logDirectory, _lock);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }

    public sealed class AndroidFileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logDirectory;
        private readonly object _lock;

        public AndroidFileLogger(string categoryName, string logDirectory, object lockObject)
        {
            _categoryName = categoryName;
            _logDirectory = logDirectory;
            _lock = lockObject;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoOpDisposable.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"[{timestamp}] [{logLevel}] [{threadId}] {_categoryName}");
            logEntry.AppendLine($"  {message}");
            
            if (exception != null)
            {
                logEntry.AppendLine($"  Exception: {exception.GetType().Name}: {exception.Message}");
                logEntry.AppendLine($"  StackTrace: {exception.StackTrace}");
            }
            
            logEntry.AppendLine(); // Empty line for readability

            WriteToFile(logEntry.ToString());
        }

        private void WriteToFile(string logEntry)
        {
            try
            {
                lock (_lock)
                {
                    var fileName = $"app_{DateTime.Now:yyyy-MM-dd}.log";
                    var filePath = Path.Combine(_logDirectory, fileName);
                    
                    File.AppendAllText(filePath, logEntry, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidFileLogger: Failed to write log: {ex.Message}");
            }
        }
    }
}