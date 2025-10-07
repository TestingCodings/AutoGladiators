# AutoGladiators Debug Log Access Guide

## 🎯 Problem Solved
You can now access debug logs from your AutoGladiators game! No more invisible logging - everything is now stored in accessible locations with multiple ways to retrieve the data.

## 📁 Log Locations

### Android (Debug Build)
```
/sdcard/AutoGladiators/Logs/
├── app_2025-10-07.log              # Main application logs
├── error_2025-10-07.log            # Error-only logs  
├── battle_2025-10-07.log           # Battle system logs
├── LOG_ACCESS_INFO.txt             # Access instructions
└── README.txt                      # Additional info
```

### Windows (Development)
```
C:\Users\[Username]\Documents\AutoGladiators\Logs\
├── app_2025-10-07.log              # Main application logs
├── error_2025-10-07.log            # Error-only logs
├── battle_2025-10-07.log           # Battle system logs
└── LOG_ACCESS_INFO.txt             # Access instructions
```

## 🚀 Quick Access Methods

### Method 1: In-Game Log Access (Easiest)
1. Open AutoGladiators
2. Go to **Main Menu** → **🐛 Debug Menu**  
3. Tap **📄 Access Debug Logs**
4. Use the **Generate Test Logs** button to create sample entries
5. Copy ADB commands or directory paths directly from the app

### Method 2: ADB Command Line
```bash
# Connect to device
adb shell

# Navigate to logs
cd /sdcard/AutoGladiators/Logs

# List all log files
ls -la

# View recent logs (last 50 lines)
cat app_2025-10-07.log | tail -50

# View error logs only
cat error_2025-10-07.log

# View battle logs
cat battle_2025-10-07.log

# Search for specific text
grep -i "error" app_2025-10-07.log

# Exit shell
exit
```

### Method 3: Pull Logs to PC
```bash
# Pull entire log directory to your PC
adb pull /sdcard/AutoGladiators/Logs ./AutoGladiators_Logs

# Pull specific file
adb pull /sdcard/AutoGladiators/Logs/app_2025-10-07.log ./game.log
```

### Method 4: Android Studio Device File Explorer  
1. Open **Android Studio**
2. **View** → **Tool Windows** → **Device File Explorer**
3. Navigate to: **sdcard** → **AutoGladiators** → **Logs**
4. Right-click any `.log` file → **Save As**
5. Choose location on your PC to save

### Method 5: SCRCPY + File Manager App
1. Install `scrcpy` for screen mirroring
2. Use any Android file manager app
3. Navigate to `/sdcard/AutoGladiators/Logs`
4. Share files to email/cloud storage

## 📊 Log Types and Content

### Main Application Log (`app_YYYY-MM-DD.log`)
```
[2025-10-07 14:30:15.123] [INFO ] [T01] AutoGladiators.Client.Pages.MainMenuPage
    MainMenuPage initialized successfully

[2025-10-07 14:30:16.456] [DEBUG] [T01] AutoGladiators.Core.Services.PlayerProfileService  
    Loading saved profiles from storage

[2025-10-07 14:30:16.789] [ERROR] [T01] AutoGladiators.Client.Pages.StarterSelectionPage
    Failed to navigate to exploration page
    EXCEPTION: NullReferenceException
    MESSAGE: Object reference not set to an instance of an object
    STACK:
      at StarterSelectionPage.OnStartGameClicked() line 123
```

### Error Log (`error_YYYY-MM-DD.log`)
- Contains only WARNING, ERROR, and CRITICAL level messages
- Perfect for quick debugging of issues
- Shows stack traces for exceptions

### Battle Log (`battle_YYYY-MM-DD.log`)  
- All battle-related logging from combat system
- Move selections, damage calculations, AI decisions
- Battle state transitions and results

## 🧪 Testing Log System

Use the **Generate Test Logs** button in the log access page to create sample entries:

```
=== GENERATING TEST LOGS ===
This is a DEBUG level message for testing file logging system
This is an INFO level message - normal application flow  
This is a WARNING level message - something might be wrong
This is an ERROR level message - something went wrong

User Action: TestAction by TestUser123
Battle Event: TestBattle - Player: Player1 vs Enemy: Enemy1  
Performance: TestOperation completed in 1250ms
=== TEST LOG GENERATION COMPLETE ===
```

## 🔧 Technical Implementation

### File Logger Provider
- **CrossPlatformFileLoggerProvider**: Handles platform-specific log directories
- **Thread-safe**: Multiple threads can log simultaneously
- **Daily rotation**: New log file each day
- **UTF-8 encoding**: Proper character support
- **Automatic directory creation**: Creates log folders if missing

### Log Levels
```
TRACE < DEBUG < INFO < WARN < ERROR < CRITICAL
```

### Structured Logging
- **Timestamps**: Precise to milliseconds
- **Thread IDs**: Track multi-threading issues  
- **Component Names**: Know exactly where logs came from
- **Exception Details**: Full stack traces when errors occur

## 🚨 Troubleshooting

### "No log files found"
1. Ensure you're in DEBUG build mode
2. Use **Generate Test Logs** to create initial files
3. Check Android permissions for external storage
4. Try navigating the game to generate natural logs

### "Permission denied" in ADB
1. Enable **Developer Options** on device
2. Enable **USB Debugging**  
3. Grant permission when prompted
4. Try `adb devices` to verify connection

### "Directory not found"
1. The log directory is created on first app startup
2. Run the app at least once in DEBUG mode
3. Check the in-game log access page for actual path
4. Permissions may prevent directory creation

### Can't pull files via ADB
```bash
# Check if directory exists
adb shell ls -la /sdcard/AutoGladiators/

# Check permissions  
adb shell ls -la /sdcard/AutoGladiators/Logs/

# Alternative path (if main path fails)
adb shell ls -la /sdcard/Android/data/com.testingcodings.autogladiators/files/
```

## 💡 Pro Tips

1. **Generate Test Logs First**: Always use the test log generator to verify the system works
2. **Check Multiple Log Types**: Error logs are great for debugging, but main logs show full context  
3. **Use grep for Filtering**: `grep -i "battle" app_*.log` to find specific events
4. **Monitor in Real-Time**: Use `adb logcat` for live debugging during development
5. **Log Rotation**: Files are created daily, so check the correct date
6. **In-Game Access**: Use the in-game log access page - it shows exactly where files are stored

## ✅ Verification Steps

1. ✅ Build and run AutoGladiators in DEBUG mode
2. ✅ Navigate: Main Menu → Debug Menu → Access Debug Logs  
3. ✅ Tap "Generate Test Logs" button
4. ✅ Tap "Refresh File List" - should show new `.log` files
5. ✅ Copy ADB commands from the app
6. ✅ Use ADB or Android Studio to verify files exist
7. ✅ Open log files and confirm readable content with timestamps

**Log access is now fully functional! 🎉**