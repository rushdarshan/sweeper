# Build Instructions for ScreenshotSweeper

This document explains how to build and create an installer for ScreenshotSweeper.

## Prerequisites

### Required
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Windows 10/11** - For WPF support

### Optional (for installer)
- **Inno Setup 6** - [Download here](https://jrsoftware.org/isdl.php)
  - Required only if you want to create the installer package

## Quick Build

### Option 1: Using PowerShell Script (Recommended)

```powershell
# Build app and create installer
.\Build-Installer.ps1

# Build only (skip installer)
.\Build-Installer.ps1 -SkipInstaller

# Create installer only (skip build)
.\Build-Installer.ps1 -SkipBuild
```

### Option 2: Manual Build

```powershell
# Restore dependencies
dotnet restore

# Build for development/testing
dotnet build

# Build for release
dotnet publish -c Release -r win-x64 --self-contained false
```

## Creating the Installer

### Step 1: Build the Application
```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

### Step 2: Run Inno Setup Compiler
```powershell
# If Inno Setup is installed at default location:
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" Setup.iss
```

The installer will be created in the `Installer` folder:
```
Installer/ScreenshotSweeper-Setup-v2.0.0.exe
```

## Output Locations

- **Development Build**: `bin\Debug\net8.0-windows10.0.19041.0\`
- **Release Build**: `bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\`
- **Installer**: `Installer\ScreenshotSweeper-Setup-v2.0.0.exe`

## Testing the Build

### Run from Command Line
```powershell
cd bin\Release\net8.0-windows10.0.19041.0\win-x64\publish
.\ScreenshotSweeper.exe
```

### Test the Installer
1. Run the installer: `Installer\ScreenshotSweeper-Setup-v2.0.0.exe`
2. Follow the installation wizard
3. Launch the app from Start Menu or Desktop shortcut

## Installer Features

The created installer includes:
- ✅ .NET 8 Runtime check (prompts to download if missing)
- ✅ Custom application icon
- ✅ Desktop shortcut option
- ✅ Start with Windows option
- ✅ Start Menu shortcuts
- ✅ Proper uninstaller with data cleanup option
- ✅ Modern wizard interface

## Troubleshooting

### "MSBuild not found"
Install Visual Studio Build Tools or full Visual Studio.

### "dotnet command not found"
Install .NET 8 SDK and restart your terminal.

### "ISCC.exe not found"
Install Inno Setup 6 from https://jrsoftware.org/isdl.php

### Build fails with icon error
Ensure `Resources\AppIcon.ico` exists in the project folder.

## Distribution

After building the installer, you can distribute:
- The single `.exe` installer file (recommended)
- Or the entire `publish` folder contents (no installer needed)

The installer is approximately 50-80 MB depending on dependencies.
