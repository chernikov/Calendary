@echo off
REM Build script for Calendary project

echo ========================================
echo Building Calendary Project
echo ========================================

echo.
echo [1/4] Restoring Backend Dependencies...
dotnet restore Calendary.sln
if %ERRORLEVEL% neq 0 (
    echo ERROR: Backend restore failed
    exit /b %ERRORLEVEL%
)

echo.
echo [2/4] Building Backend...
dotnet build Calendary.sln --configuration Release --no-restore
if %ERRORLEVEL% neq 0 (
    echo ERROR: Backend build failed
    exit /b %ERRORLEVEL%
)

echo.
echo [3/4] Installing Frontend Dependencies...
cd src\Calendary.Ng
call npm install
if %ERRORLEVEL% neq 0 (
    echo ERROR: Frontend npm install failed
    cd ..\..
    exit /b %ERRORLEVEL%
)

echo.
echo [4/4] Building Frontend...
call npm run build
if %ERRORLEVEL% neq 0 (
    echo ERROR: Frontend build failed
    cd ..\..
    exit /b %ERRORLEVEL%
)

cd ..\..

echo.
echo ========================================
echo Build completed successfully!
echo Backend: Release build
echo Frontend: Production build
echo ========================================
