# ScreenshotSweeper 🎯

*A smart, lightweight Windows app that automatically cleans up your screenshots.*

ScreenshotSweeper keeps your screenshots folder clutter-free by automatically deleting temporary screenshots after a set time, while letting you permanently keep the important ones. It runs quietly in the background and gives you full control through toast notifications, presets, and a clean modern UI.

---

![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![Version](https://img.shields.io/badge/Version-1.0.0-green)
![Framework](https://img.shields.io/badge/.NET-6.0_LTS-purple)
![License](https://img.shields.io/badge/License-MIT-orange)

---

## 📥 Download

Get the latest release from:
👉 **[Releases](https://github.com/rushdarshan/sweeper/releases)**

Available:

* `ScreenshotSweeper-Setup.exe` (Windows Installer)
* Portable version (ZIP)

---

## 🚀 Quick Start

1. Download and install ScreenshotSweeper
2. Launch the app
3. Select your **Screenshots folder**
4. Choose a delete time (or use presets)
5. Click **Save Settings**
6. Done. ScreenshotSweeper runs in the background.

---

## ✨ Features

* 🔔 **Windows Toast Notifications**
  With quick-action buttons:

  * 15 Min
  * 30 Min
  * 1 Hour
  * Keep

* ⏱️ **Flexible Deletion Timers**
  Set time in Minutes, Hours, or Days

* 🎯 **One-Click Presets**
  No typing, no configuration overhead

* 📂 **Keep Folder**
  Important screenshots are safe forever

* 🔧 **System Tray Mode**
  Runs silently in the background

* 💾 **Real-Time Monitoring**
  Powered by `FileSystemWatcher`

* ⚙️ **Custom Settings**

  * Startup behavior
  * Notification control
  * Pause / Resume monitoring
  * File type filters

* 🎨 **Clean Modern UI**

  * Setup
  * Monitor
  * Settings
  * Guide

---

## 📸 Screenshots


### Monitor Tab
<img width="1920" height="1149" alt="image" src="https://github.com/user-attachments/assets/d646ea4c-57e2-4647-bed6-061cbb9a847c" />

### Settings Tab
<img width="1920" height="1149" alt="image" src="https://github.com/user-attachments/assets/9611ae78-e56b-47d0-b598-eb302a164a33" />



---

## 🧭 How It Works

1. You take a screenshot
2. ScreenshotSweeper detects it instantly
3. A toast notification appears
4. Choose a preset or Keep
5. Timer starts
6. File auto-deletes when time expires
7. Kept files stay in the Keep folder

Simple. Fast. Zero manual cleanup.

---

## 🗂 Tabs Overview

### 🛠 Setup

* Choose screenshots folder
* Set deletion time
* Save configuration

### 📊 Monitor

* See all tracked screenshots
* Live countdown timers
* Keep / Delete Now buttons
* Color-coded urgency

### ⚙ Settings

* Enable/disable notifications
* Launch on startup
* Pause/resume monitoring
* Select file extensions

### 📘 Guide

* Help
* FAQ
* Design philosophy

---

## ⚙ Configuration Location

```
%APPDATA%\ScreenshotSweeper\config.json
```

---

## 🧪 Tested On

* Windows 10 (21H2+)
* Windows 11

---

## 🛠 Built With

| Component       | Tech                      |
| --------------- | ------------------------- |
| Language        | C#                        |
| Framework       | .NET 6 (LTS)              |
| UI              | WPF                       |
| Notifications   | Windows Community Toolkit |
| File Monitoring | FileSystemWatcher         |

---

## 🗺 Roadmap

* [x] File monitoring
* [x] Auto deletion
* [x] Presets
* [x] Toast actions
* [x] Tray icon

Upcoming:

* [ ] Installer improvements
* [ ] UI polish
* [ ] Dark/Light themes
* [ ] Custom preset editor
* [ ] Analytics dashboard

---

## 🤝 Contributing

Contributions are welcome!

You can help by:

* Improving UI
* Writing unit tests
* Creating installer scripts
* Adding new features

Open an issue or submit a pull request.

---

## 🎯 Why ScreenshotSweeper?

Because screenshots are temporary by nature.
Your storage should treat them that way.

> **Take freely. Keep only what matters. Delete the rest automatically.**
