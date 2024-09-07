@echo off

setlocal

goto main

:install
    setlocal enabledelayedexpansion
        set installName=%~1
        set installFolder=%~2

        echo Installing SsmsExtensions into %installName%.    

        if not exist "%installFolder%\ssms.exe" (
            echo - Application executable not found.
            echo - Skipping installation.
            goto :installExit
        )
        
        if not exist "%installFolder%\Extensions\." (
            echo - Creating Extensions folders.

            md "%installFolder%\Extensions" > nul
            if %ERRORLEVEL% neq 0 goto :installError
        ) else (
            echo - Extensions folders already exist.
        )

        if exist "%installFolder%\Extensions\SsmsExtensions\." (
            echo - Removing SsmsExtensions folder.
            del /s /q "%installFolder%\Extensions\SsmsExtensions\*" > nul 2>nul
            rmdir /s /q "%installFolder%\Extensions\SsmsExtensions" > nul 2>nul
            if exist "%installFolder%\Extensions\SsmsExtensions\." goto :installError
        ) else (
            echo - SsmsExtensions folder does no exist.
        )

        echo - Creating SsmsExtensions folders.

        md "%installFolder%\Extensions\SsmsExtensions" > nul 2>nul
        if not exist "%installFolder%\Extensions\SsmsExtensions\." goto :installError

        echo - Copying files.

        xcopy ".\bin\Release\*" "%installFolder%\Extensions\SsmsExtensions" /s /i > nul 2>nul
        if %ERRORLEVEL% neq 0 goto :installError

        echo - Installation completed successfully.
    endlocal
    :installExit
    exit /b 0
    :installError
    echo - Installation failed.
    exit /b %ERRORLEVEL%

:main

::snet session 1>nul 2>nul || (echo This script requires administrator privileges. & Exit /b 1)

for %%p in (%*) do (
    if [%%p]==[-AutoClose] set local_AutoClose=1
)

set local_DefaultInstallFolder19=

call:install "SSMS v18" "%ProgramFiles(x86)%\Microsoft SQL Server Management Studio 18\Common7\IDE"
call:install "SSMS v19" "%ProgramFiles(x86)%\Microsoft SQL Server Management Studio 19\Common7\IDE"

if [%local_AutoClose%]==[] pause