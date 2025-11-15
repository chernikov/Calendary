@echo off
REM Build and test script for Calendary project

echo ========================================
echo Build and Test - Calendary Project
echo ========================================

call %~dp0build.bat
if %ERRORLEVEL% neq 0 (
    echo ERROR: Build step failed
    exit /b %ERRORLEVEL%
)

echo.
call %~dp0test.bat
if %ERRORLEVEL% neq 0 (
    echo ERROR: Test step failed
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================
echo Build and Test completed successfully!
echo ========================================
