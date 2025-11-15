@echo off
REM Test Frontend only

echo ========================================
echo Running Frontend Tests (Angular)
echo ========================================

cd src\Calendary.Ng

call npm test -- --watch=false --browsers=ChromeHeadless
if %ERRORLEVEL% neq 0 (
    echo ERROR: Tests failed
    cd ..\..
    exit /b %ERRORLEVEL%
)

cd ..\..

echo.
echo Frontend tests passed successfully!
