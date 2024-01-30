@echo off
setlocal

::CHECK ADMIN

for %%p in (%*) do (
    if [%%p]==[-AutoClose] set local_AutoClose=1
)

set local_DefaultInstallFolder18=%ProgramFiles(x86)%\Microsoft SQL Server Management Studio 18\Common7\IDE
set local_DefaultInstallFolder19=%ProgramFiles(x86)%\Microsoft SQL Server Management Studio 19\Common7\IDE

set local_DefaultInstallFolder=%local_DefaultInstallFolder19%

if exist "%local_DefaultInstallFolder%\ssms.exe" goto SsmsFound

::TODO Select folder

:SsmsFound
echo:
echo SSMS found
::MAKE EXTENSIONS DIR IF NOT EXISTS
::MAKE SSMSEXTENSIONS DIR IF NOT EXISTS
::EMPTY SSMSEXTENSIONS FOLDER
::COPY FILES
::xcopy X:\Development\Github\nikolajhalvorsen\_SsmsExtensions4'\src\SsmsExtensions\bin\Debug

goto Exit
    
:Exit
if [%local_AutoClose%]==[1] goto RealExit
echo:
echo Close the window.
pause > nul

:RealExit