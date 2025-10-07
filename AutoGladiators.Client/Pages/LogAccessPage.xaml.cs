using Microsoft.Maui.Controls;
using System;
using System.IO;
using System.Text;
using AutoGladiators.Core.Services.Logging;

namespace AutoGladiators.Client.Pages
{
    public partial class LogAccessPage : ContentPage
    {
        private static readonly IAppLogger Log = AppLog.For<LogAccessPage>();
        private string _logDirectory = string.Empty;

        public LogAccessPage()
        {
            InitializeComponent();
            InitializeLogInfo();
        }

        private void InitializeLogInfo()
        {
            try
            {
                // Get the log directory based on platform
                _logDirectory = GetLogDirectory();
                
                LogDirectoryLabel.Text = _logDirectory;
                
                // Update ADB commands with current date
                var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                AdbViewCommand.Text = $"adb shell\n" +
                                      $"cd /sdcard/AutoGladiators/Logs\n" +
                                      $"ls -la\n" +
                                      $"cat app_{currentDate}.log | tail -50";
                
                RefreshFileList();
                
                Log.Info("LogAccessPage initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize LogAccessPage: {ex.Message}", ex);
                LogDirectoryLabel.Text = $"Error: {ex.Message}";
            }
        }

        private string GetLogDirectory()
        {
#if ANDROID
            var externalStorageDir = Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath;
            if (string.IsNullOrEmpty(externalStorageDir))
            {
                externalStorageDir = Platform.CurrentActivity?.GetExternalFilesDir(null)?.AbsolutePath 
                    ?? "/sdcard/Android/data/com.testingcodings.autogladiators/files";
            }
            return Path.Combine(externalStorageDir, "AutoGladiators", "Logs");
#elif WINDOWS
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.Combine(documentsPath, "AutoGladiators", "Logs");
#else
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appDataPath, "AutoGladiators", "Logs");
#endif
        }

        private void RefreshFileList()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                    LogFilesLabel.Text = "Log directory created. No files yet.";
                    return;
                }

                var files = Directory.GetFiles(_logDirectory, "*.log");
                var infoFiles = Directory.GetFiles(_logDirectory, "*.txt");
                
                var fileList = new StringBuilder();
                fileList.AppendLine($"Directory: {_logDirectory}");
                fileList.AppendLine();
                
                if (files.Length == 0 && infoFiles.Length == 0)
                {
                    fileList.AppendLine("No log files found.");
                    fileList.AppendLine("Generate test logs to create files.");
                }
                else
                {
                    fileList.AppendLine($"Found {files.Length} log files:");
                    foreach (var file in files)
                    {
                        var info = new FileInfo(file);
                        fileList.AppendLine($"  📄 {Path.GetFileName(file)} ({info.Length} bytes, {info.LastWriteTime:HH:mm:ss})");
                    }
                    
                    if (infoFiles.Length > 0)
                    {
                        fileList.AppendLine();
                        fileList.AppendLine($"Info files:");
                        foreach (var file in infoFiles)
                        {
                            fileList.AppendLine($"  📋 {Path.GetFileName(file)}");
                        }
                    }
                }

                LogFilesLabel.Text = fileList.ToString();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to refresh file list: {ex.Message}", ex);
                LogFilesLabel.Text = $"Error accessing files: {ex.Message}";
            }
        }

        private async void OnCopyPathClicked(object sender, EventArgs e)
        {
            try
            {
                // Use MAUI clipboard API which works on all platforms
                await Clipboard.SetTextAsync(_logDirectory);
                await DisplayAlert("Copied!", $"Log directory path copied to clipboard:\n{_logDirectory}", "OK");
                Log.Info("Log directory path copied to clipboard");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to copy path to clipboard: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to copy to clipboard: {ex.Message}", "OK");
            }
        }

        private async void OnCopyAdbClicked(object sender, EventArgs e)
        {
            try
            {
                var adbCommands = "# View logs in ADB shell\n" +
                                  "adb shell\n" +
                                  "cd /sdcard/AutoGladiators/Logs\n" +
                                  "ls -la\n" +
                                  $"cat app_{DateTime.Now:yyyy-MM-dd}.log | tail -50\n" +
                                  "exit\n\n" +
                                  "# Pull logs to PC\n" +
                                  "adb pull /sdcard/AutoGladiators/Logs ./AutoGladiators_Logs";

                await Clipboard.SetTextAsync(adbCommands);
                await DisplayAlert("Copied!", "ADB commands copied to clipboard!", "OK");
                Log.Info("ADB commands copied to clipboard");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to copy ADB commands: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to copy commands: {ex.Message}", "OK");
            }
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            RefreshFileList();
            Log.Info("Log file list refreshed");
        }

        private async void OnGenerateTestLogsClicked(object sender, EventArgs e)
        {
            try
            {
                Log.Info("=== GENERATING TEST LOGS ===");
                Log.Debug("This is a DEBUG level message for testing file logging system");
                Log.Info("This is an INFO level message - normal application flow");
                Log.Warn("This is a WARNING level message - something might be wrong");
                Log.Error("This is an ERROR level message - something went wrong", new Exception("Test exception for logging"));
                
                // Log some game-specific test data
                Log.LogUserAction("TestAction", "TestUser123", new Dictionary<string, object?>
                {
                    ["TestData"] = "Sample test data",
                    ["Timestamp"] = DateTime.Now,
                    ["Version"] = "1.0.0-debug"
                });
                
                Log.LogBattleEvent("TestBattle", "Player1", "Enemy1", new Dictionary<string, object?>
                {
                    ["BattleDuration"] = "45 seconds",
                    ["Winner"] = "Player1",
                    ["Moves"] = 8
                });
                
                Log.LogPerformance("TestOperation", 1250, new Dictionary<string, object?>
                {
                    ["Operation"] = "Database query",
                    ["Records"] = 100
                });
                
                Log.Info("=== TEST LOG GENERATION COMPLETE ===");
                
                // Refresh the file list to show new logs
                RefreshFileList();
                
                await DisplayAlert("Success!", 
                    "Test logs generated successfully!\n\n" +
                    "Check the log files using ADB or Android Studio Device File Explorer.", 
                    "OK");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to generate test logs: {ex.Message}", ex);
                await DisplayAlert("Error", $"Failed to generate test logs: {ex.Message}", "OK");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}