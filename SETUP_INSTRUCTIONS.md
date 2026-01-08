# 🔧 ScreenshotSweeper - Complete Setup Instructions

## 📋 System Requirements

- **OS:** Windows 10 (version 1809+) or Windows 11
- **Memory:** 2 GB RAM minimum
- **Storage:** 500 MB free (for SDK + build)

## 🚀 SETUP STEP-BY-STEP

### Step 1: Install .NET 6 SDK (Required)

**Windows:**

1. Go to: https://dotnet.microsoft.com/download/dotnet/6.0
2. Click "Download .NET 6.0 SDK" (not Runtime)
3. Choose Windows x64 or x86 installer
4. Run the installer
5. Follow prompts and complete installation
6. **Restart your computer** (important!)

**Verify Installation:**
```powershell
# Open PowerShell and type:
dotnet --version

# Should show: 6.0.xxx
```

---

### Step 2: Install Visual Studio 2022 (Recommended)

**Why?** Much easier than command line, better debugging

1. Go to: https://visualstudio.microsoft.com/vs/
2. Click "Download Visual Studio 2022 Community" (free)
3. Run installer
4. Select "Desktop development with C++" workload
5. In "Installation details", ensure these are checked:
   - ☑️ .NET Framework 4.8 SDK
   - ☑️ .NET 6.0 Runtime
   - ☑️ C# and Visual Basic Roslyn compilers
   - ☑️ Windows Forms development tools
   - ☑️ WPF development tools
6. Click **Install** (this takes 10-15 minutes)
7. Restart when prompted

---

### Step 3: Open ScreenshotSweeper Project

**In Visual Studio:**
1. Open Visual Studio 2022
2. File → Open → Project/Solution
3. Navigate to: `c:\Users\rushd\Downloads\rail madad\ScreenshotSweeper\`
4. Select: `ScreenshotSweeper.csproj`
5. Click **Open**

**Visual Studio will:**
- Automatically restore NuGet packages
- Analyze the code
- Set up IntelliSense
- Get ready to build (takes 1-2 minutes)

---

### Step 4: Build the Project

**In Visual Studio Menu:**
1. Build → Clean Solution
2. Build → Build Solution

**Watch the Output window** - should show:
```
Build started...
Restoring packages...
Building ScreenshotSweeper.csproj...
Build succeeded!
```

**If you see red errors:**
- Right-click project → Rebuild
- Or Tools → NuGet Package Manager → Package Manager Console
  - Type: `Update-Package -Reinstall`

---

### Step 5: Run the Application

**Press F5** (or Debug → Start Debugging)

**First Launch:**
- App window appears
- 4 tabs visible: Setup, Monitor, Settings, Guide
- Setup tab shows empty folder path
- System tray icon appears

---

## 🎯 First Run Configuration

### Initial Setup (30 seconds)

1. **Setup Tab** loads automatically
2. Click **Browse...** button
3. Select a folder for monitoring (e.g., `C:\Users\YourName\Pictures\Screenshots`)
   - Or create a new test folder
4. Choose deletion timer:
   - Default: 30 Minutes
   - Or use preset: **[15 Min]**, **[30 Min]**, **[1 Hour]**, **[3 Days]**
5. Click **Save Settings**
6. ✅ App is now ready!

---

## 🧪 Test It (2 minutes)

### Test 1: Screenshot Detection

1. Take a screenshot (Windows key + Shift + S)
2. Windows will show screenshot tool
3. Create a screenshot and save
4. Go to **Monitor Tab** in app
5. ✅ Screenshot should appear in list with countdown

### Test 2: Toast Notifications

1. Take another screenshot
2. ✅ Windows Toast notification should appear
3. Toast shows: Filename, Size, Remaining time
4. Toast has buttons: [15 Min], [30 Min], [1 Hour], [Keep]

### Test 3: Preset Buttons

1. In Setup Tab, click **[1 Hour]**
2. ✅ Number field changes to "1"
3. ✅ Dropdown changes to "Hours"
4. Click **[3 Days]**
5. ✅ Number field changes to "3"
6. ✅ Dropdown changes to "Days"

### Test 4: System Tray

1. Look at system tray (bottom right of taskbar)
2. ✅ Icon with app name appears
3. Hover mouse over icon
4. ✅ Tooltip shows: "ScreenshotSweeper" + file count
5. Right-click icon
6. ✅ Menu appears with: Open App, Pause Monitoring, Settings, Exit

### Test 5: Keep Folder

1. Take a screenshot
2. In Monitor Tab, click **[Keep]** button
3. ✅ File moves to Keep folder
4. ✅ File disappears from Monitor list
5. ✅ File will never be auto-deleted

---

## ⚙️ Configuration

### Settings Location

All settings saved to:
```
%APPDATA%\ScreenshotSweeper\config.json
```

**To access:**
1. Press Windows key + R
2. Type: `%APPDATA%`
3. Open ScreenshotSweeper folder
4. config.json contains all settings

### Example config.json

```json
{
  "Version": "2.0",
  "ScreenshotFolderPath": "C:\\Users\\YourName\\Pictures\\Screenshots",
  "KeepFolderPath": "C:\\Users\\YourName\\Pictures\\Screenshots\\Keep",
  "DeleteThresholdValue": 30,
  "DeleteThresholdUnit": 0,
  "Notifications": {
    "ShowOnDetection": true,
    "ShowOnDeletion": true,
    "PlaySound": false
  },
  "Startup": {
    "LaunchOnStartup": false,
    "StartMinimized": true
  },
  "Monitoring": {
    "IsActive": true
  }
}
```

---

## 🐛 Troubleshooting

### "No .NET SDKs were found"
**Problem:** .NET 6 SDK not installed  
**Solution:** 
1. Go to https://dotnet.microsoft.com/download/dotnet/6.0
2. Download and install .NET 6 SDK
3. Restart your PC

### "Cannot find 'dotnet' command"
**Problem:** .NET not in PATH  
**Solution:** Restart PowerShell/Terminal after installing SDK

### Build fails with compilation errors
**Problem:** Project won't compile  
**Solution:**
1. Visual Studio → Tools → NuGet Package Manager → Package Manager Console
2. Type: `Update-Package -Reinstall`
3. Build → Clean Solution
4. Build → Build Solution

### App launches but freezes
**Problem:** App is unresponsive  
**Solution:**
1. Check folder path exists and has write permissions
2. Try selecting a different folder in Setup tab
3. Close and reopen app

### Notifications not showing
**Problem:** Toast notifications don't appear  
**Solution:**
1. Windows Settings → System → Notifications
2. Ensure "Show notifications from apps" is ON
3. Find ScreenshotSweeper in list and enable
4. Restart app

### System tray icon not visible
**Problem:** Icon doesn't appear in system tray  
**Solution:**
1. Minimize the main window to tray
2. Check taskbar notification area (bottom right)
3. You may need to expand hidden icons

---

## 📊 What Happens Behind the Scenes

### On App Startup
1. App.xaml.cs runs
2. Services initialize (FileMonitor, Cleanup, Notification, Tray)
3. ConfigService loads settings from config.json
4. FileMonitorService starts watching folder
5. CleanupService timer starts (10 second intervals)
6. UI loads with 4 tabs
7. System tray icon appears

### When You Take a Screenshot
1. File saved to monitored folder
2. FileSystemWatcher detects it (~100ms)
3. FileMonitorService validates file type
4. CleanupService creates metadata with timer
5. NotificationService sends Toast
6. UI updates in Monitor tab
7. Tray icon updates file count

### When Timer Expires
1. CleanupService timer checks files (every 10 seconds)
2. Expired files deleted
3. Deletion notification sent
4. Monitor tab updates
5. Tray updates status

---

## ✅ Verification Checklist

After setup, verify:

- [ ] .NET 6 SDK installed (`dotnet --version` shows 6.0.x)
- [ ] Visual Studio 2022 opens project without errors
- [ ] Build succeeds (Build → Build Solution shows "Build succeeded!")
- [ ] App launches without crashes
- [ ] All 4 tabs visible (Setup, Monitor, Settings, Guide)
- [ ] Can select folder via Browse button
- [ ] Preset buttons work correctly
- [ ] Settings save and persist
- [ ] Screenshots detected in real-time
- [ ] Toast notifications appear
- [ ] System tray icon visible
- [ ] App responds to all button clicks

---

## 🎓 Project Structure (Recap)

```
ScreenshotSweeper/
├── Models/           → Data structures (TimeUnit, AppConfig, Metadata)
├── Services/         → Business logic (FileMonitor, Cleanup, etc.)
├── Views/            → UI pages (Setup, Monitor, Settings, Guide)
├── Helpers/          → Utilities (TimeHelper, FileHelper, Constants)
├── App.xaml/cs       → Application entry point
├── MainWindow.xaml   → Main UI container
└── ScreenshotSweeper.csproj → Project configuration
```

---

## 📚 Documentation Files

| File | Purpose |
|------|---------|
| README.md | User guide & features |
| MASTERPLAN.md | Technical architecture |
| TESTING_GUIDE.md | Testing procedures |
| BUILD_INSTRUCTIONS.* | Quick build commands |
| FILE_MANIFEST.md | Complete file listing |

---

## 🚀 Common Next Steps

### After Successful Test

**Option 1: Use It**
- Set up with your real Screenshots folder
- Let it monitor automatically
- Enjoy automatic cleanup!

**Option 2: Customize**
- Adjust time presets
- Change UI colors
- Add more file types
- Modify notifications

**Option 3: Share It**
- Create GitHub repository
- Push code
- Create release
- Share with others

---

## 💡 Tips for Success

1. **Start fresh:** Create a new empty folder just for testing
2. **Check logs:** If something fails, look at console output
3. **Test incrementally:** Test each feature as you go
4. **Keep it simple:** Don't change code until it's working
5. **Restart when stuck:** Close and reopen the app
6. **Check settings:** Review TESTING_GUIDE.md for expected behavior

---

## 🎉 You're Ready!

```
✅ All code generated
✅ All documentation created
✅ Ready to build and test
✅ Just need .NET 6 SDK installed

Next: Install SDK → Open in VS → F5 to run!
```

---

**Installation Time:** ~20 minutes (first time)  
**Build Time:** ~30 seconds  
**Test Time:** ~5 minutes  

**Total to first run: ~25 minutes** ⏱️

---

Need help? Check TESTING_GUIDE.md for troubleshooting! 🔧
