!include "LogicLib.nsh"

Name "ezIOmeter "
OutFile "ezIOmeterSetup.exe"

; The default installation directory
InstallDir "$PROGRAMFILES\ezIOmeter"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

Function .onInit
 
  ReadRegStr $R0 HKLM \
  "Software\Microsoft\Windows\CurrentVersion\Uninstall\ezIOmeter" \
  "UninstallString"
  StrCmp $R0 "" done
 
  MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION \
  "ezIOmeter is already installed. $\n$\nClick `OK` to remove the \
  previous version or `Cancel` to cancel this install." \
  IDOK uninst
  Abort
 
;Run the uninstaller
uninst:

	ExecWait '$R0 _?=$INSTDIR' ;Do not copy the uninstaller to a temp file
 
done:
 
FunctionEnd


Section "ezIOmeter " ;No components page, name is not important
  
  SetOutPath $INSTDIR

  CreateDirectory $INSTDIR\Results
  
  File /r ..\bin\Release\IOmeter
  File /r ..\bin\Release\IOmeterConfigFiles
  File /r ..\Documentation\ezIOmeter_User_Guide.pdf
  File ..\bin\Release\MathNet.Numerics.dll
  File ..\bin\Release\MathNet.Numerics.xml
  File ..\bin\Release\ezIOmeter.exe
  File ..\bin\Release\ezIOmeter_Lib.dll
  File ..\bin\Release\settings.conf

  WriteUninstaller "uninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ezIOmeter" "DisplayName" "ezIOmeter"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ezIOmeter" "UninstallString" "$INSTDIR\uninstall.exe"
  
  ExecWait '"$INSTDIR\createEventLog.exe" /q'

SectionEnd ; end the section

Section ".NET Framework 4.5"

	SetOutPath $INSTDIR\Support
  
	DetailPrint "Installing .NET Framework 4.5"
    File Support\dotNetFx45_Full_setup.exe	
	ExecWait '"$INSTDIR\Support\dotNetFx45_Full_setup.exe" /passive /norestart'	
  
SectionEnd

Section "Start Menu Shortcuts"

	SetOutPath $INSTDIR

	CreateDirectory "$SMPROGRAMS\ezIOmeter"
	CreateShortCut "$SMPROGRAMS\ezIOmeter\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
	CreateShortCut "$SMPROGRAMS\ezIOmeter\ezIOmeter.lnk" "$INSTDIR\ezIOmeter.exe" "" "$INSTDIR\ezIOmeter.exe" 0
	
SectionEnd

Section "Uninstall"  
  
  ; End the running RUI tasks.
  ExecWait 'TASKKILL /F /IM "ezIOmeter.exe"'
  ExecWait 'TASKKILL /F /IM "IOmeter.exe"' 
  ExecWait 'TASKKILL /F /IM "Dynamo.exe"' 
    
  RMDir /r $INSTDIR\IOmeter
  RMDir /r $INSTDIR\IOmeterConfigFiles
  RMDir /r $INSTDIR\Results
  RMDir /r $INSTDIR\Documentation
  RMDir /r $INSTDIR
  RMDir /r "$SMPROGRAMS\ezIOmeter"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ezIOmeter"  

SectionEnd