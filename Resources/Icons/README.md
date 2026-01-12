Icon generation

1) Place your provided logo image (the one you attached) at:

   Resources/Icons/source.png

2) From the project root run (PowerShell):

```powershell
pwsh .\Resources\Icons\generate-icon.ps1 -Source .\Resources\Icons\source.png -Out .\Resources\Icons\sweeper.ico
```

This creates `sweeper.ico` plus individual PNGs at multiple sizes.

3) To use the icon in the WPF project, set the `ApplicationIcon` property in `ScreenshotSweeper.csproj` (or via Project Properties) to:

   Resources/Icons/sweeper.ico

If you want, I can automatically add the icon to the project file and update the tray/icon references — tell me and I'll do it.
