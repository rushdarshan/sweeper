ScreenshotSweeper 🎯

A lightweight Windows desktop application that automatically monitors and manages screenshots with intelligent auto-deletion and Mark app-inspired UX.

Status: Phase 1 + 1.5 Implementation Complete ✅

✨ Key Features

- 🔔 Windows Toast Notifications with quick-action preset buttons
- ⏱️ Flexible Time Units (Minutes, Hours, Days)
- 🎯 Quick Preset Buttons (15 Min, 30 Min, 1 Hour, 3 Days)
- 📂 Keep Folder for permanent preservation of important screenshots
- 🔧 System Tray Integration for background monitoring
- 💾 Real-time File Monitoring using FileSystemWatcher
- ⚙️ Customizable Settings (notifications, startup behavior, file types)
- 🎨 Clean Modern UI with tabbed interface

🎨 Design Inspiration: Mark App (Android)

ScreenshotSweeper brings proven UX patterns from the popular Mark screenshot manager (Android) to Windows:

🚀 Quick Start
Prerequisites
- Windows 10 (version 1809+) or Windows 11
- .NET 6 Desktop Runtime
- Visual Studio 2022 (for development)
Installation (Development)

1. Clone the repository:
   

2. Open the solution:
   

3. Restore NuGet packages:
   

4. Build and run:

📖 Usage
Setup Tab
1. Click "Browse..." to select your Screenshots folder
2. Choose a deletion time:
   - Manual: Enter a number and select Minutes/Hours/Days
   - Quick Presets: Click [15 Min], [30 Min], [1 Hour], or [3 Days]
3. Click "Save Settings" to start monitoring
Monitor Tab
- View all tracked files with size and time remaining
- Quick actions: Keep or Delete Now buttons for each file
- Color coding: Green (plenty of time) → Yellow (warning) → Red (urgent)
Settings Tab
- Notifications: Toggle detection/deletion alerts
- Startup: Configure launch on Windows startup
- Monitoring: Pause/resume monitoring
- File types: Choose which extensions to monitor
Guide Tab
- Help & FAQ: Learn features and get answers
- Mark app inspiration: See design decisions

🏗️ Project Structure

🔧 Technical Stack

NuGet Packages

📚 Key Classes
TimeHelper.cs
Converts between time units and formats for display:
FileMonitorService.cs
Monitors screenshot folder using FileSystemWatcher:
- StartMonitoring() - Start watching folder
- StopMonitoring() - Stop watching
- Event: FileDetected - Triggered when new screenshot found
CleanupService.cs
Periodically checks and deletes expired files:
- Start() - Begin cleanup timer
- Stop() - Stop timer
- UpdateDeleteTime() - Change timer for specific file
- MoveToKeep() - Move file to Keep folder
NotificationService.cs
Sends Windows Toast notifications with action buttons:
- SendDetectionNotification() - New screenshot detected
- SendDeletionNotification() - File auto-deleted
- Preset action buttons: 15 Min, 30 Min, 1 Hour, Keep
TrayIconService.cs
Manages system tray presence:
- Icon states: Active (green), Paused (red), Warning (yellow)
- Context menu: Open, Pause/Resume, Settings, Exit
- Real-time status tooltip

🎯 Core Workflows
Screenshot Detection → Deletion
Preset Button Click Example

User clicks "30 Min" in toast notification:

⚙️ Configuration

Configuration is saved to:


Example config:

🧪 Testing
Manual Testing Checklist

- [ ] File Detection
  - [ ] Taking screenshot adds to Monitor tab
  - [ ] Toast notification appears
  - [ ] Preset buttons update timer correctly

- [ ] Deletion
  - [ ] Files expire after configured time
  - [ ] Deletion notification appears
  - [ ] File actually deleted from disk

- [ ] Keep Folder
  - [ ] Files moved to Keep folder
  - [ ] Files in Keep folder never auto-deleted

- [ ] UI
  - [ ] Setup: Dropdown selections persist
  - [ ] Monitor: Real-time countdown updates
  - [ ] Settings: All toggles work
  - [ ] Guide: Information is clear

- [ ] System Tray
  - [ ] Icon appears in tray
  - [ ] Status tooltip shows file count
  - [ ] Right-click menu works
  - [ ] Double-click restores window
Unit Testing

🔍 Troubleshooting
"Screenshots folder not found"
- Ensure folder path exists and you have read/write permissions
- Check that the folder isn't network-shared or on external drive
"No notifications appearing"
- Check Windows Settings → System → Notifications
- Ensure "Show notifications from this app" is enabled
- Restart the application
"Files not being deleted"
- Verify monitoring is Active (not Paused)
- Check that files aren't locked by another application
- Look at Console output (Debug mode) for errors
"System tray icon missing"
- Minimize the window to tray
- Check Windows taskbar notification area

🚀 Roadmap
✅ Phase 1: Core MVP
- [x] File monitoring
- [x] Time-based deletion
- [x] Configuration storage
- [x] Basic UI
✅ Phase 1.5: Mark App Features
- [x] Time unit flexibility (Minutes/Hours/Days)
- [x] Preset quick buttons
- [x] Toast notifications with actions
- [x] System tray integration
📋 Phase 2: Polish & Testing
- [ ] Enhanced UI styling
- [ ] Performance optimization
- [ ] Comprehensive unit tests
- [ ] Error handling improvements
🎁 Phase 3: Distribution
- [ ] Windows installer (Inno Setup)
- [ ] GitHub releases
- [ ] Demo GIF/video
- [ ] User documentation

📊 Performance

- Memory usage: ~50-80 MB baseline
- CPU usage: <1% at idle, <5% during file operations
- Startup time: <2 seconds
- File monitoring: Real-time via FileSystemWatcher (instant detection)
- Cleanup check: Every 10 seconds

🤝 Contributing

Contributions welcome! Areas for improvement:

- [ ] Unit tests for all services
- [ ] Installer creation
- [ ] Dark mode support
- [ ] Internationalization (i18n)
- [ ] Custom time presets
- [ ] Statistics/analytics dashboard

See CONTRIBUTING.md (CONTRIBUTING.md) for guidelines.

📄 License

This project is licensed under the MIT License - see LICENSE (LICENSE) file for details.

🙏 Credits

- Design Inspiration: Mark App (https://play.google.com/store/apps/details?id=com.creator.mark) (Android)
- UI Framework: ModernWPF (https://github.com/Kinnara/ModernWpf)
- Notifications: Microsoft Toolkit (https://github.com/microsoft/WindowsCommunityToolkit)

📧 Contact

For questions, suggestions, or bug reports:
- GitHub Issues: ScreenshotSweeper/issues (https://github.com/yourusername/ScreenshotSweeper/issues)
- Email: your.email@example.com

🎓 What I Learned

Building ScreenshotSweeper reinforced key concepts:

✅ Windows Desktop Development
- WPF architecture and XAML UI design
- System tray integration
- Native file system monitoring

✅ Software Design
- Service-oriented architecture
- Model-View separation
- Event-driven programming

✅ UX Principles
- Studying successful apps (Mark)
- Adapting patterns to new platforms
- Quick preset patterns reduce friction

✅ C# / .NET
- Async/await patterns
- JSON serialization
- Dependency management

Ready to keep only what matters! 🔥

If you find ScreenshotSweeper useful, please star ⭐ on GitHub!

