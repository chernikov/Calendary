@echo off
REM Docker Build Script for Calendary Project
REM Usage: docker-build.bat [backend|frontend|consumer|all] [tag] [push] [production]

setlocal enabledelayedexpansion

set COMPONENT=%1
set TAG=%2
set PUSH=%3
set PRODUCTION=%4

if "%COMPONENT%"=="" set COMPONENT=all
if "%TAG%"=="" set TAG=latest

echo =========================================
echo   Calendary Docker Build Script
echo =========================================
echo Component: %COMPONENT%
echo Tag: %TAG%
echo =========================================

REM Call PowerShell script with parameters
set PS_ARGS=-Component %COMPONENT% -Tag %TAG%

if "%PUSH%"=="push" (
    set PS_ARGS=%PS_ARGS% -Push
)

if "%PRODUCTION%"=="production" (
    set PS_ARGS=%PS_ARGS% -Production
)

powershell -ExecutionPolicy Bypass -File "%~dp0docker-build.ps1" %PS_ARGS%

if errorlevel 1 (
    echo Build failed!
    exit /b 1
)

echo Build completed successfully!
exit /b 0
