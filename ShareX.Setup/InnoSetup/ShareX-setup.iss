#define MyAppName "ShareX"
#ifndef Platform
  #define Platform "x64"
#endif
#if Platform == "arm64"
  #define RuntimeId "win-arm64"
  #define MyArchitecturesAllowed "arm64"
#else
  #define RuntimeId "win-x64"
  #define MyArchitecturesAllowed "x64compatible"
#endif
#define MyAppRootDirectory "..\.."
#define MyAppOutputDirectory MyAppRootDirectory + "\Output"
#define MyAppReleaseDirectory MyAppRootDirectory + "\" + MyAppName + "\bin\Release\" + RuntimeId
#define MyAppFileName MyAppName + ".exe"
#define MyAppFilePath MyAppReleaseDirectory + "\" + MyAppFileName
#define MyAppVersion GetStringFileInfo(MyAppFilePath, "ProductVersion")
#define MyAppPublisher "ShareX Team"
#define MyAppURL "https://getsharex.com"
#define MyAppId "82E6AC09-0FEF-4390-AD9F-0DD3F5561EFC"

[Setup]
AppCopyright=Copyright (c) 2007-2026 ShareX Team
AppId={#MyAppId}
AppMutex={#MyAppId}
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppVerName={#MyAppName} {#MyAppVersion}
AppVersion={#MyAppVersion}
ArchitecturesAllowed={#MyArchitecturesAllowed}
ArchitecturesInstallIn64BitMode={#MyArchitecturesAllowed}
DefaultDirName={commonpf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile={#MyAppRootDirectory}\LICENSE.txt
MinVersion=10.0.14393
OutputBaseFilename={#MyAppName}-{#MyAppVersion}-setup-{#Platform}
OutputDir={#MyAppOutputDirectory}
PrivilegesRequired=none
SolidCompression=yes
UninstallDisplayIcon={app}\{#MyAppFileName}
UninstallDisplayName={#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoTextVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}

[Tasks]
Name: "CreateDesktopIcon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Check: not IsUpdating and not DesktopIconExists
Name: "CreateStartupIcon"; Description: "Run ShareX when Windows starts"; GroupDescription: "Other tasks:"; Check: not IsUpdating
Name: "DisablePrintScreenKeyForSnippingTool"; Description: "Disable Print Screen key for Snipping Tool"; GroupDescription: "Other tasks:"; Check: not IsUpdating

[Files]
Source: "{#MyAppReleaseDirectory}\*.exe"; DestDir: {app}; Excludes: "ShareX.NativeMessagingHost.exe"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\*.dll"; DestDir: {app}; Excludes: "ShareX.UploadersLib.dll,ShareX.NativeMessagingHost.dll,FluentFTP.dll,Renci.SshNet.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\*.json"; DestDir: {app}; Excludes: "host-manifest-chrome.json,host-manifest-firefox.json,ShareX.NativeMessagingHost.deps.json,ShareX.NativeMessagingHost.runtimeconfig.json"; Flags: ignoreversion
Source: "{#MyAppRootDirectory}\Licenses\*.txt"; DestDir: {app}\Licenses; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ShareX_File_Icon.ico"; DestDir: {app}; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ar-YE\*.resources.dll"; DestDir: {app}\Languages\ar-YE; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\de\*.resources.dll"; DestDir: {app}\Languages\de; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\es\*.resources.dll"; DestDir: {app}\Languages\es; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\es-MX\*.resources.dll"; DestDir: {app}\Languages\es-MX; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\fa-IR\*.resources.dll"; DestDir: {app}\Languages\fa-IR; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\fr\*.resources.dll"; DestDir: {app}\Languages\fr; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\he-IL\*.resources.dll"; DestDir: {app}\Languages\he-IL; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\hu\*.resources.dll"; DestDir: {app}\Languages\hu; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\id-ID\*.resources.dll"; DestDir: {app}\Languages\id-ID; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\it-IT\*.resources.dll"; DestDir: {app}\Languages\it-IT; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ja-JP\*.resources.dll"; DestDir: {app}\Languages\ja-JP; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ko-KR\*.resources.dll"; DestDir: {app}\Languages\ko-KR; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\nl-NL\*.resources.dll"; DestDir: {app}\Languages\nl-NL; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\pl\*.resources.dll"; DestDir: {app}\Languages\pl; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\pt-BR\*.resources.dll"; DestDir: {app}\Languages\pt-BR; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\pt-PT\*.resources.dll"; DestDir: {app}\Languages\pt-PT; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ro\*.resources.dll"; DestDir: {app}\Languages\ro; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ru\*.resources.dll"; DestDir: {app}\Languages\ru; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\tr\*.resources.dll"; DestDir: {app}\Languages\tr; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\uk\*.resources.dll"; DestDir: {app}\Languages\uk; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\vi-VN\*.resources.dll"; DestDir: {app}\Languages\vi-VN; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\zh-CN\*.resources.dll"; DestDir: {app}\Languages\zh-CN; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\zh-TW\*.resources.dll"; DestDir: {app}\Languages\zh-TW; Excludes: "ShareX.UploadersLib.resources.dll"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppFileName}"; WorkingDir: "{app}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"; WorkingDir: "{app}"
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppFileName}"; WorkingDir: "{app}"; Tasks: CreateDesktopIcon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppFileName}"; WorkingDir: "{app}"; Parameters: "-silent"; Tasks: CreateStartupIcon

[Run]
Filename: "{app}\{#MyAppFileName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall; Check: not IsNoRun

[Registry]
Root: "HKCU"; Subkey: "Software\Classes\.sxie"; Flags: dontcreatekey uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\ShareX.sxie"; Flags: dontcreatekey uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\SystemFileAssociations\image\shell\ShareXImageEditor"; Flags: dontcreatekey uninsdeletekey
Root: "HKCU"; Subkey: "Control Panel\Keyboard"; ValueType: dword; ValueName: "PrintScreenKeyForSnippingEnabled"; ValueData: "0"; Flags: uninsdeletevalue; Tasks: DisablePrintScreenKeyForSnippingTool

[InstallDelete]
Type: files; Name: "{app}\ShareX.UploadersLib.dll"
Type: files; Name: "{app}\ShareX.UploadersLib.pdb"
Type: files; Name: "{app}\ShareX.NativeMessagingHost.exe"
Type: files; Name: "{app}\ShareX.NativeMessagingHost.exe.config"
Type: files; Name: "{app}\ShareX.NativeMessagingHost.dll"
Type: files; Name: "{app}\ShareX.NativeMessagingHost.pdb"
Type: files; Name: "{app}\ShareX.NativeMessagingHost.deps.json"
Type: files; Name: "{app}\ShareX.NativeMessagingHost.runtimeconfig.json"
Type: files; Name: "{app}\host-manifest-chrome.json"
Type: files; Name: "{app}\host-manifest-firefox.json"
Type: files; Name: "{app}\FluentFTP.dll"
Type: files; Name: "{app}\Renci.SshNet.dll"
Type: files; Name: "{app}\Licenses\FluentFTP_license.txt"
Type: files; Name: "{app}\Licenses\SSH.NET_license.txt"
Type: files; Name: "{app}\Languages\ar-YE\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\de\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\es\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\es-MX\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\fa-IR\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\fr\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\he-IL\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\hu\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\id-ID\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\it-IT\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\ja-JP\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\ko-KR\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\nl-NL\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\pl\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\pt-BR\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\pt-PT\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\ro\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\ru\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\tr\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\uk\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\vi-VN\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\zh-CN\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\zh-TW\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\cs\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\da\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\hi\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\sv\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{app}\Languages\th\ShareX.UploadersLib.resources.dll"
Type: files; Name: "{usersendto}\{#MyAppName}.lnk"

[Code]
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\*\shell\{#MyAppName}');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\Directory\shell\{#MyAppName}');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\.sxcu');
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Classes\ShareX.sxcu');
    RegDeleteKeyIncludingSubkeys(HKCU, 'SOFTWARE\Google\Chrome\NativeMessagingHosts\com.getsharex.sharex');
    RegDeleteKeyIncludingSubkeys(HKCU, 'SOFTWARE\Mozilla\NativeMessagingHosts\ShareX');
  end;
end;

procedure InitializeWizard;
begin
  if not IsAdmin then
  begin
    WizardForm.DirEdit.Text := ExpandConstant('{userpf}\{#MyAppName}');
  end;
end;

function InitializeUninstall(): Boolean;
var
  ErrorCode: Integer;
begin
  if CheckForMutexes('{#MyAppId}') then
  begin
    if MsgBox('Uninstall has detected that {#MyAppName} is currently running.' + #13#10#13#10 + 'Would you like to close it?', mbError, MB_YESNO) = IDYES then
    begin
      Exec('taskkill.exe', '/f /im {#MyAppFileName}', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
    end
    else
    begin
      Result := False;
      Exit;
    end;
  end;

  Result := True;
end;

function CmdLineParamExists(const value: string): Boolean;
var
  i: Integer;
begin
  Result := False;
  for i := 1 to ParamCount do
    if CompareText(ParamStr(i), value) = 0 then
    begin
      Result := True;
      Exit;
    end;
end;

function IsUpdating(): Boolean;
begin
  Result := CmdLineParamExists('/UPDATE');
end;

function IsNoRun(): Boolean;
begin
  Result := CmdLineParamExists('/NORUN');
end;

function DesktopIconExists(): Boolean;
begin
  Result := FileExists(ExpandConstant('{userdesktop}\{#MyAppName}.lnk'));
end;
