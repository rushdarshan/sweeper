# ScreenshotSweeper Build and Package Script
# This script builds the application and creates an installer

param(
    [switch]$SkipBuild,
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ScreenshotSweeper Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectDir = $PSScriptRoot
$PublishDir = Join-Path $ProjectDir "bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"
$InstallerDir = Join-Path $ProjectDir "Installer"

# Step 1: Clean previous builds
if (-not $SkipBuild) {
    Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path "bin") { Remove-Item -Path "bin" -Recurse -Force }
    if (Test-Path "obj") { Remove-Item -Path "obj" -Recurse -Force }
    if (Test-Path $InstallerDir) { Remove-Item -Path $InstallerDir -Recurse -Force }
    Write-Host "  ✓ Cleaned" -ForegroundColor Green
    Write-Host ""
}

# Step 2: Restore dependencies
if (-not $SkipBuild) {
    Write-Host "[2/4] Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Restore failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ✓ Packages restored" -ForegroundColor Green
    Write-Host ""
}

# Step 3: Build and publish
if (-not $SkipBuild) {
    Write-Host "[3/4] Building application..." -ForegroundColor Yellow
    dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=false /p:IncludeNativeLibrariesForSelfExtract=true
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ✗ Build failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "  ✓ Build completed" -ForegroundColor Green
    Write-Host ""
}

# Step 4: Create installer
if (-not $SkipInstaller) {
    Write-Host "[4/4] Creating installer..." -ForegroundColor Yellow
    
    # Check if Inno Setup is installed
    $InnoSetupPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe"
    )
    
    $ISCC = $null
    foreach ($path in $InnoSetupPaths) {
        if (Test-Path $path) {
            $ISCC = $path
            break
        }
    }
    
    # Use the manually curated InstallerInfo.rtf for Inno Setup InfoBeforeFile
    Write-Host "  ✓ Using InstallerInfo.rtf for setup information" -ForegroundColor Green

    if ($null -eq $ISCC) {
        Write-Host "  ⚠ Inno Setup not found!" -ForegroundColor Yellow
        Write-Host "  Please install Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
        Write-Host "  Then run this script again." -ForegroundColor Yellow
    } else {
        & $ISCC "Setup.iss"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ✗ Installer creation failed!" -ForegroundColor Red
            exit 1
        }
        Write-Host "  ✓ Installer created" -ForegroundColor Green
        
        # Show installer location
        $InstallerFile = Get-ChildItem -Path $InstallerDir -Filter "*.exe" | Select-Object -First 1
        if ($InstallerFile) {
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Cyan
            Write-Host "  Build Complete!" -ForegroundColor Green
            Write-Host "========================================" -ForegroundColor Cyan
            Write-Host "Installer location:" -ForegroundColor White
            Write-Host "  $($InstallerFile.FullName)" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "Installer size: $([math]::Round($InstallerFile.Length / 1MB, 2)) MB" -ForegroundColor White
        }
    }
} else {
    Write-Host "[4/4] Skipping installer creation" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Done! 🎉" -ForegroundColor Green
