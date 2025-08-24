@echo off
echo =======================================
echo Starting FDX.Trading Multi-Portal Platform...
echo =======================================
echo.

REM Set environment to Development
set ASPNETCORE_ENVIRONMENT=Development

echo Starting Admin Portal (http://localhost:5193)...
start "FoodX Admin Portal" cmd /k "cd FoodX.Admin && dotnet run"
timeout /t 2 /nobreak > nul

echo Starting Buyer Portal (http://localhost:5000)...
start "FoodX Buyer Portal" cmd /k "cd FoodX.Buyer && dotnet run"
timeout /t 2 /nobreak > nul

echo Starting Supplier Portal (http://localhost:5001)...
start "FoodX Supplier Portal" cmd /k "cd FoodX.Supplier && dotnet run"
timeout /t 2 /nobreak > nul

echo Starting Marketplace (http://localhost:5002)...
start "FoodX Marketplace" cmd /k "cd FoodX.Marketplace && dotnet run"

echo.
echo =======================================
echo All portals are starting...
echo =======================================
echo.
echo Portal URLs:
echo   Admin:       http://localhost:5193
echo   Buyer:       http://localhost:5000
echo   Supplier:    http://localhost:5001
echo   Marketplace: http://localhost:5002
echo.
echo Close each command window to stop the portals
echo =======================================
pause