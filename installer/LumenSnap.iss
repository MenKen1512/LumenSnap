#ifndef AppVersion
  #define AppVersion "0.1.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish-0.1.0"
#endif
#ifndef OutputDir
  #define OutputDir "..\artifacts\dist"
#endif
[Setup]
AppId={{E7201EB6-CE0A-4C13-9736-4E7C70B29A0C}
AppName=LumenSnap
AppVersion={#AppVersion}
AppVerName=LumenSnap {#AppVersion}
AppPublisher=LumenSnap
AppPublisherURL=https://github.com/MenKen1512/LumenSnap
DefaultDirName={localappdata}\Programs\LumenSnap
DefaultGroupName=LumenSnap
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.22000
OutputDir={#OutputDir}
OutputBaseFilename=LumenSnap-Setup-{#AppVersion}-win-x64
SetupIconFile=..\artifacts\app.ico
UninstallDisplayIcon={app}\LumenSnap.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
CloseApplications=no
RestartApplications=no
AppMutex=Local\LumenSnap-{username}
VersionInfoVersion={#AppVersion}.0
LicenseFile=..\LICENSE
[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; Flags: unchecked
[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\LumenSnap"; Filename: "{app}\LumenSnap.exe"
Name: "{autodesktop}\LumenSnap"; Filename: "{app}\LumenSnap.exe"; Tasks: desktopicon
[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "LumenSnap"; Flags: uninsdeletevalue
[Code]
var PurgeSettings: Boolean;
function InitializeUninstall(): Boolean;
begin
  Result := True;
  PurgeSettings := ExpandConstant('{param:PURGESETTINGS|0}') = '1';
  if not UninstallSilent then
    PurgeSettings := MsgBox('Удалить также личные настройки LumenSnap?', mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES;
end;
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if (CurUninstallStep = usPostUninstall) and PurgeSettings then
    DelTree(ExpandConstant('{localappdata}\LumenSnap'), True, True, True);
end;
