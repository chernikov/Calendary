@echo off
REM Production Docker Build Script for Calendary Project
REM Usage: docker-build-production.bat [backend|frontend|consumer|all] [tag] [nopush]

setlocal enabledelayedexpansion

set COMPONENT=%1
set TAG=%2
set NOPUSH=%3

if "%COMPONENT%"=="" set COMPONENT=all
if "%TAG%"=="" set TAG=latest

echo =========================================
echo   Calendary PRODUCTION Build
echo =========================================
echo Component: %COMPONENT%
echo Tag: %TAG%
echo Target: calendary.com.ua
echo =========================================

REM Call PowerShell script with parameters
set PS_ARGS=-Component %COMPONENT% -Tag %TAG%

if "%NOPUSH%"=="nopush" (
    set PS_ARGS=%PS_ARGS% -Push:$false
)

powershell -ExecutionPolicy Bypass -File "%~dp0docker-build-production.ps1" %PS_ARGS%

if errorlevel 1 (
    echo Production build failed!
    exit /b 1
)

echo Production build completed successfully!
exit /b 0
