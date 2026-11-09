; Nova Control Center — client installer (Inno Setup 6)
; One double-click setup.exe: installs the self-contained app (bundled .NET 8
; + self-contained Windows App Runtime) and, as a safety net, runs the
; official Windows App Runtime 1.6 installer silently if the runtime is
; missing on the client PC. No .NET install needed from the client.

#define MyAppName "Nova Control Center"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "YasinmkDev"
#define MyAppExeName "NovaControlCenter.exe"

[Setup]
AppId={{5F5FB0D6-37A2-499B-834D-9A29017EFF99}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=..\installer-output
OutputBaseFilename=NovaControlCenter-Setup-1.0.0
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "windowsappruntimeinstall-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{tmp}\windowsappruntimeinstall-x64.exe"; Parameters: "--quiet"; StatusMsg: "Installing Windows App Runtime (one-time, only if missing)..."; Flags: waituntilterminated
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
