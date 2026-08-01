using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.Services.Logging
{
    internal sealed class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }

    public sealed class CrossPlatformFileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public CrossPlatformFileLoggerProvider()
        {
            _logDirectory = GetLogDirectory();
            
            try
            {
                Directory.CreateDirectory(_logDirectory);
                
                // Write platform info and access instructions
                var infoPath = Path.Combine(_logDirectory, "LOG_ACCESS_INFO.txt");
                var platformInfo = GetPlatformAccessInfo();
                File.WriteAllText(infoPath, platformInfo, Encoding.UTF8);
                    
                System.Diagnostics.Debug.WriteLine($"CrossPlatformFileLogger: Logs available at {_logDirectory}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CrossPlatformFileLogger: Setup failed: {ex.Message}");
            }
        }

        private string GetLogDirectory()
        {
#if ANDROID
            try
            {
                // Use app-specific external directory that doesn't require MANAGE_EXTERNAL_STORAGE permission
                var context = Platform.CurrentActivity ?? Android.App.Application.Context;
                var externalFilesDir = context.GetExternalFilesDir(null)?.AbsolutePath;
                
                if (!string.IsNullOrEmpty(externalFilesDir))
                {
                    return Path.Combine(externalFilesDir, "AutoGladiators", "Logs");
                }
                
                // Fallback to app's private cache directory
                var cacheDir = context.CacheDir?.AbsolutePath;
                if (!string.IsNullOrEmpty(cacheDir))
                {
                    return Path.Combine(cacheDir, "AutoGladiators", "Logs");
                }
                
                // Final fallback
                return Path.Combine("/data/data/com.cortexa.autogladiators", "AutoGladiators", "Logs");
            }
            catch (Exception)
            {
                // Emergency fallback to app data
                return Path.Combine(Path.GetTempPath(), "AutoGladiators", "Logs");
            }
#elif WINDOWS
            // Windows: Use Documents folder for easy access
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, "AutoGladiators", "Logs");
#else
            // Other platforms: Use app data directory
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appDataPath, "AutoGladiators", "Logs");
#endif
        }

        private string GetPlatformAccessInfo()
        {
            var info = new StringBuilder();
            info.AppendLine("AutoGladiators Debug Logs");
            info.AppendLine("========================");
            info.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            info.AppendLine($"Directory: {_logDirectory}");
            info.AppendLine();

#if ANDROID
            info.AppendLine("ANDROID ACCESS METHODS:");
            info.AppendLine("======================");
            info.AppendLine();
            info.AppendLine("Method 1 - ADB Command Line (Debug Build):");
            info.AppendLine("  adb shell");
            info.AppendLine("  run-as com.companyname.autogladiators_maui");
            info.AppendLine("  cd files/AutoGladiators/Logs");
            info.AppendLine("  ls -la");
            info.AppendLine("  cat app_YYYY-MM-DD.log | head -50");
            info.AppendLine("  exit");
            info.AppendLine();
            info.AppendLine("Method 2 - Android Studio Device File Explorer:");
            info.AppendLine("  1. Open Android Studio");
            info.AppendLine("  2. View → Tool Windows → Device File Explorer");
            info.AppendLine("  3. Navigate to data/data/com.companyname.autogladiators_maui/files/AutoGladiators/Logs");
            info.AppendLine("  4. Right-click files → Save As");
            info.AppendLine();
            info.AppendLine("Method 3 - ADB Pull (may require root):");
            info.AppendLine("  adb shell run-as com.companyname.autogladiators_maui cp files/AutoGladiators/Logs/*.log /sdcard/");
            info.AppendLine("  adb pull /sdcard/app_*.log .");
            info.AppendLine();
            info.AppendLine("Note: App-specific storage doesn't require external storage permissions");
#elif WINDOWS
            info.AppendLine("WINDOWS ACCESS METHODS:");
            info.AppendLine("======================");
            info.AppendLine();
            info.AppendLine("Method 1 - File Explorer:");
            info.AppendLine($"  Navigate to: {_logDirectory}");
            info.AppendLine();
            info.AppendLine("Method 2 - Command Prompt:");
            info.AppendLine($"  cd \"{_logDirectory}\"");
            info.AppendLine("  dir");
            info.AppendLine("  type app_YYYY-MM-DD.log");
            info.AppendLine();
            info.AppendLine("Method 3 - PowerShell:");
            info.AppendLine($"  Set-Location \"{_logDirectory}\"");
            info.AppendLine("  Get-Content app_*.log | Select-Object -Last 50");
#endif
            info.AppendLine();
            info.AppendLine("Log File Naming:");
            info.AppendLine("  app_YYYY-MM-DD.log - Main application logs");
            info.AppendLine("  battle_YYYY-MM-DD.log - Battle system logs");  
            info.AppendLine("  error_YYYY-MM-DD.log - Error-only logs");
            info.AppendLine();
            info.AppendLine("Log Levels: TRACE < DEBUG < INFO < WARN < ERROR < CRITICAL");
            
            return info.ToString();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CrossPlatformFileLogger(categoryName, _logDirectory, _lock);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }

    public sealed class CrossPlatformFileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logDirectory;
        private readonly object _lock;

        public CrossPlatformFileLogger(string categoryName, string logDirectory, object lockObject)
        {
            _categoryName = categoryName;
            _logDirectory = logDirectory;
            _lock = lockObject;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new EmptyDisposable();

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"[{timestamp}] [{logLevel,-5}] [T{threadId:D2}] {_categoryName}");
            logEntry.AppendLine($"    {message}");
            
            if (exception != null)
            {
                logEntry.AppendLine($"    EXCEPTION: {exception.GetType().Name}");
                logEntry.AppendLine($"    MESSAGE: {exception.Message}");
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    logEntry.AppendLine($"    STACK:");
                    var stackLines = exception.StackTrace.Split('\n');
                    foreach (var line in stackLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                            logEntry.AppendLine($"      {line.Trim()}");
                    }
                }
            }
            
            logEntry.AppendLine(); // Empty line for readability

            // Write to appropriate log file based on level and category
            WriteToFile(logEntry.ToString(), logLevel);
            
            // Also write to debug console for immediate visibility during development
            System.Diagnostics.Debug.WriteLine($"[{logLevel}] {_categoryName}: {message}");
        }

        private void WriteToFile(string logEntry, LogLevel logLevel)
        {
            try
            {
                lock (_lock)
                {
                    var dateStr = DateTime.Now.ToString("yyyy-MM-dd");
                    
                    // Main log file - all messages
                    var mainFile = Path.Combine(_logDirectory, $"app_{dateStr}.log");
                    File.AppendAllText(mainFile, logEntry, Encoding.UTF8);
                    
                    // Error-only log file for quick error debugging
                    if (logLevel >= LogLevel.Error)
                    {
                        var errorFile = Path.Combine(_logDirectory, $"error_{dateStr}.log");
                        File.AppendAllText(errorFile, logEntry, Encoding.UTF8);
                    }
                    
                    // Battle-specific logs for game debugging
                    if (_categoryName.Contains("Battle") || _categoryName.Contains("Combat") || _categoryName.Contains("Move"))
                    {
                        var battleFile = Path.Combine(_logDirectory, $"battle_{dateStr}.log");
                        File.AppendAllText(battleFile, logEntry, Encoding.UTF8);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CrossPlatformFileLogger: Write failed: {ex.Message}");
            }
        }
    }
}