#define MyAppName "SsmsExtensions"
#define MyAppVersion "2024.09.07.01"
#define MyAppPublisher "Nikolaj Halvorsen"
#define MyAppURL "https://github.com/nikolajhalvorsen/SsmsExtensions"

[Setup]
AppId={{C0E57F4B-34DE-418A-8FCB-FBA816FB8EF5}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={commonpf32}\Microsoft SQL Server Management Studio 19\Common7\IDE
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=.\..\..\..\LICENSE
InfoBeforeFile=.\InfoBefore.txt
OutputDir=.\..\..\..\release
OutputBaseFilename=SsmsExtensions
SetupIconFile=.\..\Icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: ".\..\bin\Release\*"; DestDir: "{app}\Extensions\SsmsExtensions"; Flags: ignoreversion recursesubdirs createallsubdirs
