@echo off
echo Cleaning up orphaned dotnet processes...
echo.

:: Kill all dotnet.exe processes except Visual Studio ServiceHub ones
for /f "tokens=2" %%i in ('tasklist ^| findstr /i "^dotnet.exe" ^| findstr /v "ServiceHub"') do (
    echo Killing dotnet.exe process with PID: %%i
    taskkill //PID %%i //F 2>nul
    if errorlevel 1 (
        echo Process %%i already terminated or access denied
    ) else (
        echo Successfully terminated process %%i
    )
)

:: Check for processes on common development ports
echo.
echo Checking for processes on common ports...
netstat -ano | findstr ":5000 :5193 :5001" 2>nul

echo.
echo Cleanup complete!
echo You can now start your application without port conflicts.
pause