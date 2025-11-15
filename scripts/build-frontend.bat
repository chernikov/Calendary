@echo off
REM Build Frontend only

echo ========================================
echo Building Frontend (Angular)
echo ========================================

cd src\Calendary.Ng

call npm install
if %ERRORLEVEL% neq 0 (
    echo ERROR: npm install failed
    cd ..\..
    exit /b %ERRORLEVEL%
)

call npm run build
if %ERRORLEVEL% neq 0 (
    echo ERROR: Build failed
    cd ..\..
    exit /b %ERRORLEVEL%
)

cd ..\..

echo.
echo Frontend build completed successfully!
