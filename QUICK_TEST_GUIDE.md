# 🎬 How to Test ScreenshotSweeper NOW

## ⏰ Quick Overview

To get the app running and testing takes about **25 minutes** total:

1. **Install .NET 6 SDK** (~5 min) - One-time setup
2. **Build project** (~2 min) - dotnet build or Visual Studio
3. **Run & test** (~3 min) - Press F5
4. **Verify features** (~5 min) - Go through checklist

---

## 🚀 FASTEST PATH: Use Visual Studio 2022

### Prerequisites (Do Once)
```
1. Download .NET 6 SDK
   Go to: https://dotnet.microsoft.com/download/dotnet/6.0
   Choose: .NET 6 SDK (Windows x64)
   Install and restart

2. Download Visual Studio 2022
   Go to: https://visualstudio.microsoft.com/vs/
   Download: Community Edition (free)
   Choose: C++ Desktop Development
   Include: WPF tools
   Install and restart
```

### Build & Run (Do Every Time)
```powershell
# 1. Open Visual Studio 2022
# 2. File → Open → Project/Solution
# 3. Select: ScreenshotSweeper.csproj
# 4. Wait for it to load (~1 minute)
# 5. Build → Build Solution
# 6. Press F5

# Done! App runs!
```

---

## 📟 ALTERNATIVE: Use Command Line

### One-Time Setup
```powershell
# Install .NET 6 SDK from:
https://dotnet.microsoft.com/download/dotnet/6.0
```

### Build & Run
```powershell
cd 'c:\Users\rushd\Downloads\rail madad\ScreenshotSweeper'

dotnet restore

dotnet build

dotnet run
```

---

## ✅ Quick Test (5 minutes)

Once app launches:

### Test 1: Setup (30 seconds)
```
1. Click [Browse...] button
2. Select any folder
3. Click [15 Min] preset button
4. Click [Save Settings]
✓ Should see "✅ Settings saved!"
```

### Test 2: Take Screenshot (1 minute)
```
1. Press Windows + Shift + S
2. Draw a screenshot
3. Wait 2 seconds
4. Look at app's Monitor tab
✓ Screenshot should appear in list with countdown
```

### Test 3: Toast Notification (30 seconds)
```
1. Take another screenshot
2. Look at screen
✓ Windows Toast notification should appear
✓ Toast has buttons: [15 Min] [30 Min] [1 Hour] [Keep]
```

### Test 4: System Tray (30 seconds)
```
1. Look at bottom right (taskbar)
✓ App icon should be visible
2. Right-click it
✓ Menu appears with: Open, Pause, Exit
```

### Test 5: Presets (30 seconds)
```
1. Back in Setup tab
2. Click [3 Days]
✓ Number becomes "3"
✓ Unit becomes "Days"
```

---

## 🎯 What Each Part Does

### Setup Tab
- **Purpose:** Configure folder and timer
- **Test:** Can you select a folder? Do presets work?

### Monitor Tab
- **Purpose:** See tracked files and countdown
- **Test:** Does screenshot appear? Does timer count down?

### Settings Tab
- **Purpose:** Control notifications and startup
- **Test:** Can you toggle Pause/Resume monitoring?

### Guide Tab
- **Purpose:** Help and documentation
- **Test:** Can you read the help text?

### System Tray
- **Purpose:** Background operation
- **Test:** Is the icon visible? Does right-click work?

### Toast Notifications
- **Purpose:** Alert user to new screenshots
- **Test:** Does the notification appear? Do buttons work?

---

## 🔍 Expected Results

### ✅ Successful Test Signs
```
✓ App launches without error
✓ Can see 4 tabs (Setup, Monitor, Settings, Guide)
✓ Can select folder via Browse
✓ Preset buttons update number and unit
✓ Settings save (see success message)
✓ Screenshot appears in Monitor tab within 2 seconds
✓ Toast notification appears (Windows native)
✓ System tray icon is visible
✓ Tray icon right-click works
✓ Monitor tab shows countdown timer
✓ App doesn't crash after 5+ minutes of use
```

### ❌ Something's Wrong If
```
✗ App crashes on startup
✗ Can't select folder
✗ Presets don't work
✗ Screenshot not detected
✗ No Toast notification
✗ System tray icon missing
✗ Monitor tab doesn't update
```

---

## 🛠️ If Something Goes Wrong

### App Won't Start
```
1. Check .NET SDK installed: dotnet --version
2. Should show: 6.0.x
3. If not, install from: https://dotnet.microsoft.com/download/dotnet/6.0
```

### Build Fails
```
1. Visual Studio: Tools → NuGet Package Manager → Restore
2. Build → Clean Solution
3. Build → Build Solution
```

### Screenshots Not Detected
```
1. Check folder path is valid
2. Check folder has write permissions
3. Check Monitoring is Active (not Paused) in Settings tab
4. Restart app
```

### No Toast Notifications
```
1. Check Windows Settings → System → Notifications
2. Find ScreenshotSweeper in notification list
3. Toggle ON
4. Restart app
```

---

## 📋 Complete Testing Workflow

```
┌─────────────────────────────────┐
│ 1. Install .NET 6 SDK (5 min)   │
│    https://dotnet.microsoft.com │
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ 2. Open Project in VS 2022      │
│    File → Open → .csproj        │
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ 3. Build Solution (2 min)       │
│    Build → Build Solution       │
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ 4. Run App (Press F5) (~1 sec)  │
│    Debug → Start Debugging      │
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ 5. Test Features (5 min)        │
│    See Quick Test above         │
└──────────────┬──────────────────┘
               ↓
┌─────────────────────────────────┐
│ 6. Verify All Tests Passed ✓    │
│    Screenshot appears in <2sec  │
│    Toast shows preset buttons   │
│    Tray icon visible & working  │
│    Timer counts down correctly  │
└─────────────────────────────────┘
```

---

## ⏱️ Time Breakdown

| Step | Time | Notes |
|------|------|-------|
| Install .NET SDK | 5 min | One-time only |
| Install Visual Studio | 15 min | One-time only (optional if CLI) |
| Open project | 1 min | VS loads and analyzes |
| Build | 2 min | First build slower, subsequent faster |
| Run app | <1 sec | ~2 seconds for app to display |
| Test features | 5 min | Go through test checklist |
| **TOTAL** | **~25 min** | **First time only** |

---

## 🎯 Test Results Template

Use this to track your testing:

```markdown
# ScreenshotSweeper Test Results

**Date:** January 8, 2026  
**Tester:** [Your Name]  

## Setup Tab
- [x] Browse button works
- [x] Preset buttons update fields
- [x] Settings save successfully
- [ ] Other: _______________

## File Detection
- [x] Screenshot detected in <2 sec
- [x] Appears in Monitor tab
- [x] Toast notification shows
- [ ] Other: _______________

## Toast Notification
- [x] Shows filename and size
- [x] Has 4 action buttons
- [x] Button clicks work
- [ ] Other: _______________

## System Tray
- [x] Icon appears in taskbar
- [x] Tooltip shows status
- [x] Right-click menu works
- [ ] Other: _______________

## Timer & Deletion
- [x] Timer counts down
- [x] Updates in real-time
- [x] Files delete after timer
- [ ] Other: _______________

## Overall Status
- [x] No crashes
- [x] Responsive UI
- [x] Settings persist
- [x] Ready to use!

**Verdict:** ✅ WORKING / ❌ NEEDS FIXES
```

---

## 💾 Project Location

```
c:\Users\rushd\Downloads\rail madad\ScreenshotSweeper\
```

**Key Files:**
- `ScreenshotSweeper.csproj` - Project file (open this in VS)
- `App.xaml.cs` - Application startup
- `MainWindow.xaml` - Main window
- `Views/` - All UI pages
- `Services/` - All business logic

---

## 📞 Need Help?

**Before Testing:**
- Read: SETUP_INSTRUCTIONS.md

**During Testing:**
- Reference: TESTING_GUIDE.md

**Understanding Code:**
- Read: README.md (user perspective)
- Read: MASTERPLAN.md (technical perspective)

---

## 🎉 Success!

Once you see:
1. ✅ App window with 4 tabs
2. ✅ Screenshot detected in <2 seconds  
3. ✅ Toast notification appears
4. ✅ Tray icon visible
5. ✅ No crashes after 5 minutes

**YOU'VE SUCCESSFULLY TESTED SCREENSHOTSWEEPER!** 🚀

---

## Next Steps

- [ ] Share with friends
- [ ] Deploy to other machines
- [ ] Customize features
- [ ] Create GitHub repo
- [ ] Build installer
- [ ] Publish release

---

**Ready? Install .NET 6 SDK → Open in VS → Press F5!** 🎬

Good luck! 🍀
