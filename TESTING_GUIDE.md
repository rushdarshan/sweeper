# 🧪 ScreenshotSweeper - Testing & Setup Guide

## ⚠️ Prerequisites Required

Before you can build and test ScreenshotSweeper, you need:

### 1️⃣ .NET 6 SDK (Required)
```
Download from: https://dotnet.microsoft.com/download/dotnet/6.0
```

**Install Steps:**
1. Go to https://dotnet.microsoft.com/download/dotnet/6.0
2. Download ".NET 6 SDK" (not Runtime)
3. Run installer and follow prompts
4. Restart your terminal/PowerShell

**Verify Installation:**
```powershell
dotnet --version
# Should show: 6.0.x
```

---

### 2️⃣ Visual Studio 2022 (Recommended)
```
Download from: https://visualstudio.microsoft.com/vs/
```

**Install Steps:**
1. Download Visual Studio 2022 Community (free)
2. Run installer
3. Select "Desktop development with C++" workload
4. Make sure these are checked:
   - [ ] .NET 6.0 Runtime
   - [ ] .NET Framework 4.8 SDK
   - [ ] WPF development tools
   - [ ] Windows Forms development tools
5. Install and restart

---

## 🚀 Quick Start (After Installing Prerequisites)

### Option A: Visual Studio 2022 (Recommended)

```powershell
# Navigate to project
cd 'c:\Users\rushd\Downloads\rail madad\ScreenshotSweeper'

# Open in Visual Studio
start ScreenshotSweeper.csproj
```

Then in Visual Studio:
1. **Build** → **Clean Solution**
2. **Build** → **Build Solution**
3. Press **F5** to run

---

### Option B: Command Line (.NET CLI)

```powershell
# Navigate to project
cd 'c:\Users\rushd\Downloads\rail madad\ScreenshotSweeper'

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run application
dotnet run
```

---

## 🧪 Testing Checklist

Once the app runs, test these features:

### 1. Setup Tab ✓
- [ ] Click **Browse...** button
- [ ] Select a folder (e.g., Pictures or Downloads)
- [ ] Observe folder path appears in text box
- [ ] Dropdown shows: Minutes, Hours, Days
- [ ] Click **[15 Min]** - number changes to 15, unit to Minutes
- [ ] Click **[30 Min]** - number changes to 30, unit to Minutes
- [ ] Click **[1 Hour]** - number changes to 1, unit to Hours
- [ ] Click **[3 Days]** - number changes to 3, unit to Days
- [ ] Click **[Save Settings]** - see success message
- [ ] Close app and reopen - settings persist

### 2. Monitor Tab ✓
- [ ] Tab shows empty message initially
- [ ] Take a screenshot (Windows key + Shift + S)
- [ ] Wait 2 seconds
- [ ] New screenshot appears in list
- [ ] Time remaining shows countdown (e.g., "29m 45s")
- [ ] Refresh happens automatically

### 3. Notifications ✓
- [ ] When screenshot detected, Windows Toast appears
- [ ] Toast shows filename and size
- [ ] Toast has 4 buttons: [15 Min], [30 Min], [1 Hour], [Keep]
- [ ] Click **[1 Hour]** in toast
- [ ] Go to Monitor tab - time updates to 1 hour

### 4. System Tray ✓
- [ ] Look in system tray (bottom right)
- [ ] Icon appears with status
- [ ] Hover over icon - tooltip shows file count
- [ ] Right-click icon - context menu appears with: Open, Pause, Exit
- [ ] Double-click icon - main window appears/restores

### 5. Settings Tab ✓
- [ ] Checkboxes toggle notifications on/off
- [ ] **Pause Monitoring** button works
- [ ] Status changes to 🔴 Red when paused
- [ ] **Resume Monitoring** button appears
- [ ] Status changes to 🟢 Green when resumed

### 6. Guide Tab ✓
- [ ] Displays Getting Started instructions
- [ ] Shows feature explanations
- [ ] Mark app inspiration visible
- [ ] FAQ section present

---

## 🐛 Troubleshooting

### Build Errors

**Error: "No .NET SDKs were found"**
- Solution: Install .NET 6 SDK from https://dotnet.microsoft.com/download/dotnet/6.0

**Error: "The type or namespace name 'Window' does not exist"**
- Solution: Make sure UseWPF=true in .csproj (already done)

**Error: NuGet packages not restoring**
- Solution in Visual Studio:
  ```
  Tools → NuGet Package Manager → Package Manager Console
  Update-Package -Reinstall
  ```

### Runtime Errors

**App won't start / crashes**
- Check Event Viewer for details
- Try running in Debug mode
- Check Console output

**Notifications not showing**
- Windows 10/11 only
- Check Settings → System → Notifications
- Ensure "Show notifications from this app" is ON

**System tray icon missing**
- Minimize the window to tray
- Look in taskbar notification area

**Files not monitored**
- Make sure folder path is valid
- Check folder has write permissions
- Verify monitoring is Active (not Paused)

---

## 📊 Expected Behavior

### Normal Operation Flow

```
1. App starts
   ↓
2. Load settings from config.json
   ↓
3. Setup tab shows last used folder
   ↓
4. FileMonitorService starts watching folder
   ↓
5. CleanupService timer starts (every 10 seconds)
   ↓
6. Take screenshot
   ↓
7. FileSystemWatcher detects new file
   ↓
8. Toast notification appears
   ↓
9. File appears in Monitor tab
   ↓
10. Timer counts down
    ↓
11. File expires → auto-deleted
    ↓
12. Deletion notification shown
```

### Console Output (Debug Mode)

When running in debug, you'll see:
```
[FileMonitor] Started monitoring: C:\Users\...\Screenshots
[CleanupService] Started cleanup timer
[FileMonitor] Detected: screenshot_2024-01-08.png
[CleanupService] Deleted: screenshot_2024-01-08.png
[TrayIconService] Initialized
```

---

## 🎯 What Each File Does

### Core Services
- **FileMonitorService**: Watches folder for new screenshots
- **CleanupService**: Deletes files after timer expires
- **NotificationService**: Shows Windows Toast alerts
- **TrayIconService**: Manages system tray icon
- **ConfigService**: Saves/loads settings to JSON

### UI Pages
- **SetupTab**: Configuration (folder, time, presets)
- **MonitorTab**: View tracked files and countdown
- **SettingsTab**: Preferences and monitoring control
- **GuideTab**: Help and FAQ

### Helpers
- **TimeHelper**: Convert between time units (min/hour/day)
- **FileHelper**: Safe file operations (delete, move, validate)
- **Constants**: App-wide settings and intervals

---

## 📝 Testing Notes

### To Test Time Units

1. **Minutes:**
   - Set 30 minutes
   - Take screenshot
   - Wait a few seconds - timer should show "29m xx s"

2. **Hours:**
   - Set 2 hours
   - Take screenshot
   - Timer should show "1h 59m xx s"

3. **Days:**
   - Set 1 day
   - Take screenshot
   - Timer should show "23h 59m xx s"

### To Test Presets

1. Go to Setup tab
2. Delete time shows: 30 Minutes
3. Click **[15 Min]**
   - Verify: shows 15 in number box
   - Verify: dropdown shows "Minutes"
4. Click **[1 Hour]**
   - Verify: shows 1 in number box
   - Verify: dropdown shows "Hours"
5. Click **[3 Days]**
   - Verify: shows 3 in number box
   - Verify: dropdown shows "Days"

### To Test Toast Actions

1. Take a screenshot
2. Toast notification appears
3. Click **[30 Min]** in toast
4. Go to Monitor tab
5. Verify the file's "Deletes In" changed to ~30 minutes

### To Test Keep Folder

1. Take a screenshot
2. Click **[Keep]** button (in Monitor tab or toast)
3. Verify file moved to Keep folder
4. Verify it no longer appears in Monitor tab
5. Verify it's never auto-deleted

---

## ✅ Successful Test Indicators

You'll know it's working correctly when:

✅ App launches without errors
✅ Settings tab shows 4 tabs (Setup, Monitor, Settings, Guide)
✅ Folder can be selected via Browse button
✅ Preset buttons auto-fill number and unit
✅ Settings save and persist
✅ Screenshots are detected in real-time
✅ Toast notifications appear
✅ Time counts down in Monitor tab
✅ System tray icon is visible
✅ Tray menu has working options
✅ Files auto-delete after timer expires

---

## 📊 Performance Expectations

| Operation | Expected Time |
|-----------|---|
| App startup | < 2 seconds |
| File detection | < 1 second |
| UI refresh | < 100ms |
| Toast notification | < 500ms |
| Cleanup cycle | ~10 seconds (configured) |

---

## 🎓 If You Get Stuck

### Check These Files First

1. **CONFIG LOCATION:**
   ```
   %APPDATA%\ScreenshotSweeper\config.json
   ```
   (You can edit this to change settings manually)

2. **PROJECT STRUCTURE:**
   - Models/ → Data structures
   - Services/ → Business logic
   - Views/ → UI pages
   - Helpers/ → Utilities

3. **BUILD OUTPUT:**
   - bin/Debug/net6.0-windows/ → Compiled files

---

## 🚀 Next Steps After Testing

1. **Everything works?**
   - ✅ Create GitHub repository
   - ✅ Push code to GitHub
   - ✅ Create release v1.0.0

2. **Find bugs?**
   - ✅ Open Issues on GitHub
   - ✅ Fix and test
   - ✅ Commit and push

3. **Want to extend?**
   - ✅ Add unit tests
   - ✅ Add more presets
   - ✅ Add dark mode
   - ✅ Add installer

---

## 📞 Getting Help

**If build fails:**
- Make sure .NET 6 SDK is installed
- Run `dotnet --version` to verify
- Try: `dotnet clean && dotnet restore && dotnet build`

**If tests fail:**
- Check Console output for error messages
- Verify folder path is valid
- Ensure app has write permissions to folder

**If you need more info:**
- See README.md - User guide
- See MASTERPLAN.md - Technical details
- See FILE_MANIFEST.md - Complete file listing

---

**Ready to test? Install .NET 6 SDK, then run: `dotnet run`** 🚀

Good luck! 🎉
