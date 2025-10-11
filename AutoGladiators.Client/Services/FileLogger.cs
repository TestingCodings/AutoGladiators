using System;
using System.IO;
using System.Threading.Tasks;

namespace AutoGladiators.Client.Services
{
    public interface IFileLogger
    {
        Task LogBattleEvent(string message);
        Task LogSystemEvent(string message);
        Task LogError(string message, Exception? exception = null);
        Task<string> GetBattleLogPath();
        Task<string> GetSystemLogPath();
        Task<string> GetErrorLogPath();
        Task<string> GetLogsDirectory();
        Task ExportBattleLog(string battleId);
    }

    public class FileLogger : IFileLogger
    {
        private readonly string _logsDirectory;
        private readonly string _battleLogFile;
        private readonly string _systemLogFile;
        private readonly string _errorLogFile;

        public FileLogger()
        {
            // Use Documents folder which is accessible on both Windows and Android
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _logsDirectory = Path.Combine(documentsPath, "AutoGladiators", "Logs");
            
            // Ensure directory exists
            Directory.CreateDirectory(_logsDirectory);
            
            // Create log file paths with date stamps
            var dateStamp = DateTime.Now.ToString("yyyy-MM-dd");
            _battleLogFile = Path.Combine(_logsDirectory, $"battle_log_{dateStamp}.txt");
            _systemLogFile = Path.Combine(_logsDirectory, $"system_log_{dateStamp}.txt");
            _errorLogFile = Path.Combine(_logsDirectory, $"error_log_{dateStamp}.txt");
        }

        public async Task LogBattleEvent(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] {message}";
            
            try
            {
                await File.AppendAllTextAsync(_battleLogFile, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Fallback - log to system log if battle log fails
                await LogSystemEvent($"Failed to write to battle log: {ex.Message}");
            }
        }

        public async Task LogSystemEvent(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] SYSTEM: {message}";
            
            try
            {
                await File.AppendAllTextAsync(_systemLogFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // If system log fails, there's not much we can do
            }
        }

        public async Task LogError(string message, Exception? exception = null)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] ERROR: {message}";
            
            if (exception != null)
            {
                logEntry += $"{Environment.NewLine}Exception: {exception}";
            }
            
            try
            {
                await File.AppendAllTextAsync(_errorLogFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Error logging failed - not much we can do
            }
        }

        public Task<string> GetBattleLogPath() => Task.FromResult(_battleLogFile);
        public Task<string> GetSystemLogPath() => Task.FromResult(_systemLogFile);
        public Task<string> GetErrorLogPath() => Task.FromResult(_errorLogFile);
        public Task<string> GetLogsDirectory() => Task.FromResult(_logsDirectory);

        public async Task ExportBattleLog(string battleId)
        {
            var exportFileName = $"battle_export_{battleId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var exportPath = Path.Combine(_logsDirectory, "Exports");
            Directory.CreateDirectory(exportPath);
            
            var exportFullPath = Path.Combine(exportPath, exportFileName);
            
            if (File.Exists(_battleLogFile))
            {
                var battleLog = await File.ReadAllTextAsync(_battleLogFile);
                var exportContent = $"=== BATTLE LOG EXPORT ==={Environment.NewLine}";
                exportContent += $"Battle ID: {battleId}{Environment.NewLine}";
                exportContent += $"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}";
                exportContent += $"================================{Environment.NewLine}{Environment.NewLine}";
                exportContent += battleLog;
                
                await File.WriteAllTextAsync(exportFullPath, exportContent);
                await LogSystemEvent($"Battle log exported to: {exportFullPath}");
            }
        }
    }
}