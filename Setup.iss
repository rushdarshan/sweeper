; ScreenshotSweeper Installer Script for Inno Setup
; Download Inno Setup from: https://jrsoftware.org/isdl.php

#define MyAppName "ScreenshotSweeper"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "darshan-aids"
#define MyAppURL "https://github.com/rushdarshan/sweeper"
#define MyAppExeName "ScreenshotSweeper.exe"

[Setup]
; App Information
AppId={{8F7A3C2E-9B1D-4F5E-A8C7-2D4E6B9F1A3C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=Installer
OutputBaseFilename=ScreenshotSweeper-Setup-v{#MyAppVersion}
; Use the generated sweeper.ico for installer/exe icons
SetupIconFile=Resources\Icons\sweeper.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesInstallIn64BitMode=x64

; Wizard Configuration
DisableProgramGroupPage=yes
DisableWelcomePage=no

; License and Info
LicenseFile=LICENSE
InfoBeforeFile=README.md

; Uninstall
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main application files
Source: "bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Include app icon files so shortcuts can reference them after install
Source: "Resources\Icons\sweeper.ico"; DestDir: "{app}\Resources\Icons"; Flags: ignoreversion
Source: "Resources\Icons\sweeper_16x16.png"; DestDir: "{app}\Resources\Icons"; Flags: ignoreversion
Source: "Resources\Icons\sweeper_32x32.png"; DestDir: "{app}\Resources\Icons"; Flags: ignoreversion
Source: "Resources\Icons\sweeper_48x48.png"; DestDir: "{app}\Resources\Icons"; Flags: ignoreversion
Source: "Resources\Icons\sweeper_64x64.png"; DestDir: "{app}\Resources\Icons"; Flags: ignoreversion
Source: "Resources\Icons\sweeper_128x128.png"; DestDir: "{app}\Resources\Icons"; Flags: ignoreversion
Source: "Resources\Icons\sweeper_256x256.png"; DestDir: "{app}\Resources\Icons"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Resources\Icons\sweeper.ico"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Resources\Icons\sweeper.ico"; Tasks: desktopicon

[Run]
; Run setup wizard after installation to configure the app
Filename: "{app}\{#MyAppExeName}"; Parameters: "--setup"; Description: "Configure ScreenshotSweeper"; Flags: nowait postinstall
; Launch the app after setup wizard completes (optional)
Filename: "{app}\{#MyAppExeName}"; Parameters: "--minimized"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent unchecked

[Code]
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
  DotNetInstalled: Boolean;
begin
  Result := True;
  
  // Check if .NET 8 Runtime is installed
  DotNetInstalled := RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost') or
                     RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedhost');
  
  if not DotNetInstalled then
  begin
    if MsgBox('.NET 8 Runtime is required but not installed.' + #13#10 + #13#10 +
              'Would you like to download it now?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0/runtime', '', '', SW_SHOW, ewNoWait, ErrorCode);
    end;
    Result := False;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create AppData folder for config
    CreateDir(ExpandConstant('{userappdata}\ScreenshotSweeper'));
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if MsgBox('Do you want to remove application data and configuration files?', mbConfirmation, MB_YESNO) = IDYES then
    begin
      DelTree(ExpandConstant('{userappdata}\ScreenshotSweeper'), True, True, True);
    end;
  end;
end;
