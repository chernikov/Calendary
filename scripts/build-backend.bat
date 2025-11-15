@echo off
REM Build Backend only

echo ========================================
echo Building Backend (.NET)
echo ========================================

dotnet restore Calendary.sln
if %ERRORLEVEL% neq 0 (
    echo ERROR: Restore failed
    exit /b %ERRORLEVEL%
)

dotnet build Calendary.sln --configuration Release --no-restore
if %ERRORLEVEL% neq 0 (
    echo ERROR: Build failed
    exit /b %ERRORLEVEL%
)

echo.
echo Backend build completed successfully!
