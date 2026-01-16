param(
    $processName='ScreenshotSweeper',
    $out='screenshot-app.png'
)

Add-Type @"
using System;
using System.Runtime.InteropServices;
public struct RECT { public int Left,Top,Right,Bottom; }
public class Win32 {
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
}
"@

$proc = Get-Process -Name $processName -ErrorAction SilentlyContinue
if (-not $proc) {
    if (Test-Path .\publish\win-x64\ScreenshotSweeper.exe) {
        Start-Process -FilePath '.\\publish\\win-x64\\ScreenshotSweeper.exe'
    } elseif (Test-Path .\bin\Release\net8.0-windows10.0.19041.0\ScreenshotSweeper.exe) {
        Start-Process -FilePath '.\\bin\\Release\\net8.0-windows10.0.19041.0\\ScreenshotSweeper.exe'
    } elseif (Test-Path .\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\ScreenshotSweeper.exe) {
        Start-Process -FilePath '.\\bin\\Release\\net8.0-windows10.0.19041.0\\win-x64\\publish\\ScreenshotSweeper.exe'
    }
    Start-Sleep -Seconds 2
    $proc = Get-Process -Name $processName -ErrorAction SilentlyContinue
}
if (-not $proc) { Write-Error 'Process not found or failed to start.'; exit 1 }

$hwnd = $proc.MainWindowHandle
$tries = 0
while ($hwnd -eq 0 -and $tries -lt 20) {
  Start-Sleep -Milliseconds 300
  $proc.Refresh(); $hwnd = $proc.MainWindowHandle; $tries++
}
if ($hwnd -eq 0) { Write-Error 'Could not get window handle.'; exit 1 }

$rect = New-Object RECT
$result = [Win32]::GetWindowRect($hwnd,[ref]$rect)
if (-not $result) { Write-Error 'GetWindowRect failed'; exit 1 }

$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top

Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap $width, $height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, ([System.Drawing.Size]::new($width,$height)))
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Output "Saved screenshot to $out"
