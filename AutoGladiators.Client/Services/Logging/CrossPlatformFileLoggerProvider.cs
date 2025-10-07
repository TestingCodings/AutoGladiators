using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AutoGladiators.Client.Services.Logging
{
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
            // Android: Use external storage accessible via ADB
            var externalStorageDir = Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath;
            if (string.IsNullOrEmpty(externalStorageDir))
            {
                // Fallback to app-specific external directory
                externalStorageDir = Platform.CurrentActivity?.GetExternalFilesDir(null)?.AbsolutePath 
                    ?? "/sdcard/Android/data/com.testingcodings.autogladiators/files";
            }
            return Path.Combine(externalStorageDir, "AutoGladiators", "Logs");
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
            info.AppendLine("Method 1 - ADB Command Line:");
            info.AppendLine("  adb shell");
            info.AppendLine("  cd /sdcard/AutoGladiators/Logs");
            info.AppendLine("  ls -la");
            info.AppendLine("  cat app_YYYY-MM-DD.log | head -50");
            info.AppendLine("  exit");
            info.AppendLine();
            info.AppendLine("Method 2 - Pull to PC:");
            info.AppendLine("  adb pull /sdcard/AutoGladiators/Logs ./AutoGladiators_Logs");
            info.AppendLine();
            info.AppendLine("Method 3 - Android Studio Device File Explorer:");
            info.AppendLine("  1. Open Android Studio");
            info.AppendLine("  2. View → Tool Windows → Device File Explorer");
            info.AppendLine("  3. Navigate to sdcard/AutoGladiators/Logs");
            info.AppendLine("  4. Right-click files → Save As");
            info.AppendLine();
            info.AppendLine("Method 4 - SCRCPY + File Manager:");
            info.AppendLine("  1. Install scrcpy for screen mirroring");
            info.AppendLine("  2. Use any Android file manager app");
            info.AppendLine("  3. Navigate to /sdcard/AutoGladiators/Logs");
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

        public IDisposable BeginScope<TState>(TState state) => null!;

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