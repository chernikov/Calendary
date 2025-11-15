@echo off
REM Test Backend only

echo ========================================
echo Running Backend Tests (.NET)
echo ========================================

dotnet test Calendary.sln --configuration Release --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo ERROR: Tests failed
    exit /b %ERRORLEVEL%
)

echo.
echo Backend tests passed successfully!
