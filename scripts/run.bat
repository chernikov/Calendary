@echo off
REM Run script for Calendary project

echo ========================================
echo Starting Calendary Application
echo ========================================

echo.
echo Starting API...
start "Calendary API" cmd /k "cd src\Calendary.Api && dotnet run"

timeout /t 3 /nobreak >nul

echo.
echo Starting Consumer...
start "Calendary Consumer" cmd /k "cd src\Calendary.Consumer && dotnet run"

timeout /t 3 /nobreak >nul

echo.
echo Starting Angular Frontend...
start "Calendary Frontend" cmd /k "cd src\Calendary.Ng && npm start"

echo.
echo ========================================
echo All services are starting...
echo Check the opened terminal windows
echo ========================================
