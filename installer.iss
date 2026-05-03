[Setup]
AppName=AutoReplayUploaderSA
AppVersion=0.1.1
AppPublisher=Suket
; Default installation directory to LocalAppData so admin rights aren't needed
DefaultDirName={localappdata}\AutoReplayUploaderSA
DefaultGroupName=AutoReplayUploaderSA
OutputDir=Output
OutputBaseFilename=AutoReplayUploaderSA_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Copy everything from the publish directory
Source: "bin\Release\net10.0-windows\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\AutoReplayUploaderSA"; Filename: "{app}\SuketAutoReplayUploader.exe"
Name: "{autodesktop}\AutoReplayUploaderSA"; Filename: "{app}\SuketAutoReplayUploader.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\SuketAutoReplayUploader.exe"; Description: "{cm:LaunchProgram,AutoReplayUploaderSA}"; Flags: nowait postinstall skipifsilent
