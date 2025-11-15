@echo off
REM Test script for Calendary project

echo ========================================
echo Running Calendary Tests
echo ========================================

echo.
echo [1/2] Running Backend Tests...
dotnet test Calendary.sln --configuration Release --no-build --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo ERROR: Backend tests failed
    exit /b %ERRORLEVEL%
)

echo.
echo [2/2] Running Frontend Tests...
cd src\Calendary.Ng
call npm test -- --watch=false --browsers=ChromeHeadless
if %ERRORLEVEL% neq 0 (
    echo ERROR: Frontend tests failed
    cd ..\..
    exit /b %ERRORLEVEL%
)

cd ..\..

echo.
echo ========================================
echo All tests passed successfully!
echo Backend: .NET tests passed
echo Frontend: Angular tests passed
echo ========================================
