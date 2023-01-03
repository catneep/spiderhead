#define MyAppName "Spiderhead"
#define MyAppVersion "1.0"
#define MyAppPublisher "catneep"
#define MyAppURL "https://www.havila.dev"
#define MyAppExeName "spiderhead.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{AFAA7356-89F9-4527-808A-BD79520E1488}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=dist
OutputBaseFilename=spiderhead-install
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "dist\net5.0-windows10.0.22000.0\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\net5.0-windows10.0.22000.0\Hardcodet.NotifyIcon.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\net5.0-windows10.0.22000.0\Microsoft.Windows.SDK.NET.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\net5.0-windows10.0.22000.0\spiderhead.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\net5.0-windows10.0.22000.0\spiderhead.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\net5.0-windows10.0.22000.0\spiderhead.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\net5.0-windows10.0.22000.0\spiderhead.runtimeconfig.dev.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\net5.0-windows10.0.22000.0\spiderhead.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\net5.0-windows10.0.22000.0\WinRT.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

