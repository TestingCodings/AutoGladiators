using AutoGladiators.Client.Services;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace AutoGladiators.Client.ViewModels
{
    public class LogViewerViewModel : INotifyPropertyChanged
    {
        private readonly IFileLogger _fileLogger = new FileLogger();
        private string _battleLogContent = "";
        private string _systemLogContent = "";
        private string _errorLogContent = "";
        private string _logFilePaths = "";

        public string BattleLogContent
        {
            get => _battleLogContent;
            set
            {
                _battleLogContent = value;
                OnPropertyChanged(nameof(BattleLogContent));
            }
        }

        public string SystemLogContent
        {
            get => _systemLogContent;
            set
            {
                _systemLogContent = value;
                OnPropertyChanged(nameof(SystemLogContent));
            }
        }

        public string ErrorLogContent
        {
            get => _errorLogContent;
            set
            {
                _errorLogContent = value;
                OnPropertyChanged(nameof(ErrorLogContent));
            }
        }

        public string LogFilePaths
        {
            get => _logFilePaths;
            set
            {
                _logFilePaths = value;
                OnPropertyChanged(nameof(LogFilePaths));
            }
        }

        public ICommand RefreshLogsCommand { get; }
        public ICommand ClearLogsCommand { get; }
        public ICommand ExportBattleLogCommand { get; }
        public ICommand OpenLogsDirectoryCommand { get; }

        public LogViewerViewModel()
        {
            RefreshLogsCommand = new Command(async () => await RefreshLogs());
            ClearLogsCommand = new Command(async () => await ClearLogs());
            ExportBattleLogCommand = new Command(async () => await ExportBattleLog());
            OpenLogsDirectoryCommand = new Command(async () => await ShowLogsPaths());

            // Load logs on startup
            _ = RefreshLogs();
        }

        private async Task RefreshLogs()
        {
            try
            {
                var battleLogPath = await _fileLogger.GetBattleLogPath();
                var systemLogPath = await _fileLogger.GetSystemLogPath();
                var errorLogPath = await _fileLogger.GetErrorLogPath();

                // Read battle log
                if (File.Exists(battleLogPath))
                {
                    BattleLogContent = await File.ReadAllTextAsync(battleLogPath);
                }
                else
                {
                    BattleLogContent = "No battle log found. Start a battle to generate logs.";
                }

                // Read system log
                if (File.Exists(systemLogPath))
                {
                    SystemLogContent = await File.ReadAllTextAsync(systemLogPath);
                }
                else
                {
                    SystemLogContent = "No system log found.";
                }

                // Read error log
                if (File.Exists(errorLogPath))
                {
                    ErrorLogContent = await File.ReadAllTextAsync(errorLogPath);
                }
                else
                {
                    ErrorLogContent = "No errors logged (this is good!).";
                }

                // Update file paths
                var logsDir = await _fileLogger.GetLogsDirectory();
                LogFilePaths = $"Logs Directory: {logsDir}\n\nBattle Log: {battleLogPath}\nSystem Log: {systemLogPath}\nError Log: {errorLogPath}";
            }
            catch (System.Exception ex)
            {
                await _fileLogger.LogError("Failed to refresh logs in viewer", ex);
                BattleLogContent = $"Error loading logs: {ex.Message}";
            }
        }

        private async Task ClearLogs()
        {
            try
            {
                var result = await Application.Current.MainPage.DisplayAlert(
                    "Clear Logs", 
                    "Are you sure you want to clear all log files?", 
                    "Yes", "Cancel");

                if (result)
                {
                    var battleLogPath = await _fileLogger.GetBattleLogPath();
                    var systemLogPath = await _fileLogger.GetSystemLogPath();
                    var errorLogPath = await _fileLogger.GetErrorLogPath();

                    if (File.Exists(battleLogPath)) File.Delete(battleLogPath);
                    if (File.Exists(systemLogPath)) File.Delete(systemLogPath);
                    if (File.Exists(errorLogPath)) File.Delete(errorLogPath);

                    await RefreshLogs();
                    
                    await Application.Current.MainPage.DisplayAlert("Success", "Logs cleared successfully!", "OK");
                }
            }
            catch (System.Exception ex)
            {
                await _fileLogger.LogError("Failed to clear logs", ex);
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to clear logs: {ex.Message}", "OK");
            }
        }

        private async Task ExportBattleLog()
        {
            try
            {
                var battleId = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                await _fileLogger.ExportBattleLog(battleId);
                
                var logsDir = await _fileLogger.GetLogsDirectory();
                var exportPath = Path.Combine(logsDir, "Exports");
                
                await Application.Current.MainPage.DisplayAlert(
                    "Export Complete", 
                    $"Battle log exported to:\n{exportPath}", 
                    "OK");
            }
            catch (System.Exception ex)
            {
                await _fileLogger.LogError("Failed to export battle log", ex);
                await Application.Current.MainPage.DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
            }
        }

        private async Task ShowLogsPaths()
        {
            try
            {
                var logsDir = await _fileLogger.GetLogsDirectory();
                
                await Application.Current.MainPage.DisplayAlert(
                    "Logs Location", 
                    $"Logs are saved to:\n{logsDir}\n\nYou can find these files in your Documents folder under AutoGladiators/Logs", 
                    "OK");
            }
            catch (System.Exception ex)
            {
                await _fileLogger.LogError("Failed to show logs directory", ex);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}