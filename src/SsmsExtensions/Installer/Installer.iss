#define MyAppName "SsmsExtensions"
#define MyAppVersion "2024.09.07.01"
#define MyAppPublisher "Nikolaj Halvorsen"
#define MyAppURL "https://github.com/nikolajhalvorsen/SsmsExtensions"
#define MyAppMutex "02bccddd-cf4f-4db9-8dd4-b3a485b9918d,Global\02bccddd-cf4f-4db9-8dd4-b3a485b9918d"
#define MySetupMutex "C0E57F4B-34DE-418A-8FCB-FBA816FB8EF5,Global\C0E57F4B-34DE-418A-8FCB-FBA816FB8EF5"

[Setup]
AppMutex={#MyAppMutex}
AppId={{C0E57F4B-34DE-418A-8FCB-FBA816FB8EF5}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
Compression=lzma
DefaultDirName={commonpf32}\Microsoft SQL Server Management Studio 19\Common7\IDE
DefaultGroupName={#MyAppName}
DirExistsWarning=no
DisableProgramGroupPage=yes
InfoBeforeFile=.\InfoBefore.txt
LicenseFile=.\..\..\..\LICENSE
OutputBaseFilename=SsmsExtensions
OutputDir=.\..\..\..\release
SetupIconFile=.\..\Icon.ico
SetupMutex=MySetupMutex
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: ".\..\bin\Release\*"; DestDir: "{app}\Extensions\SsmsExtensions"; Flags: ignoreversion recursesubdirs createallsubdirs;

[Code]
function IsAppRunning(const FileName : string): Boolean;
var
  FSWbemLocator: Variant;
  FWMIService: Variant;
  FWbemObjectSet: Variant;
begin
  FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
  FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
  FWbemObjectSet := FWMIService.ExecQuery(Format('SELECT Name FROM Win32_Process Where Name="%s"', [FileName]));
  Result := (FWbemObjectSet.Count > 0);
  FWbemObjectSet := Unassigned;
  FWMIService := Unassigned;
  FSWbemLocator := Unassigned;
end;

function EnsureAppNotRunning(AppFileName, CloseMessage, CanceledMessage: String): Boolean;
var
  MsgBoxResult: Integer;
begin
  MsgBoxResult := IDRETRY;
  while IsAppRunning(AppFileName) and (MsgBoxResult <> IDCANCEL) do
  begin
    MsgBoxResult := SuppressibleMsgBox(CloseMessage, mbError, MB_RETRYCANCEL, IDCANCEL);
  end;
  Result := Not IsAppRunning(AppFileName);
 
  if Not Result then
  begin
    SuppressibleMsgBox(CanceledMessage, mbInformation, MB_OK, IDCANCEL);
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := EnsureAppNotRunning(
    'ssms.exe', 
    'SSMS is running.' + Chr(13) + Chr(13) + 'Please close SSMS to continue installing {#MyAppName}.',
    '{#MyAppName} installlation was canceled.');
end;

function InitializeUninstall(): Boolean;
begin
  Result := EnsureAppNotRunning(
    'ssms.exe', 
    'SSMS is running.' + Chr(13) + Chr(13) + 'Please close SSMS to continue uninstalling {#MyAppName}.',
    '{#MyAppName} uninstalllation was canceled.');
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID <> wpSelectDir then
  begin
    Result := True;
    Exit;
  end;
  if FileExists(WizardDirValue + '\ssms.exe') then
  begin
    Result := True;
    Exit;
  end;
  if SuppressibleMsgBox(
    'The selected folder does not appear to be a SSMS installation folder.' + Chr(13) + Chr(13) + 'Do you want to continue installing to the selected folder?',
    mbConfirmation, MB_YESNO, IDNO) = IDYES then
  begin
    Result := True;
    Exit;
  end;
  Result := False;
end;