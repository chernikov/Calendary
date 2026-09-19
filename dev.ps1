<#
.SYNOPSIS
Runs Calendary locally for development.

.DESCRIPTION
Starts mssql + backend in Docker (EF Core migrations apply automatically on backend
startup) and runs the frontend via `ng serve` on :4200 instead of the Docker frontend
image, so it hot-reloads on save. The frontend's own :4200 docker container is stopped
first to free the port — dev-mode CORS (`Cors__AllowedOrigins__0` in docker-compose.yml)
and the Google OAuth client are both configured for http://localhost:4200, so the
frontend has to run on that exact port.

.PARAMETER SkipBuild
Skip rebuilding the backend image (faster restart when only frontend files changed).
#>

param(
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

Write-Host "Stopping docker frontend container (frees :4200 for ng serve)..." -ForegroundColor Cyan
docker compose stop frontend 2>$null | Out-Null

if ($SkipBuild) {
    Write-Host "Starting mssql + backend (no rebuild)..." -ForegroundColor Cyan
    docker compose up -d mssql backend
} else {
    Write-Host "Building and starting mssql + backend..." -ForegroundColor Cyan
    docker compose up -d --build mssql backend
}

Write-Host "Waiting for backend on http://localhost:5080 ..." -ForegroundColor Cyan
$deadline = (Get-Date).AddSeconds(90)
$ready = $false
while ((Get-Date) -lt $deadline) {
    try {
        $resp = Invoke-WebRequest -Uri 'http://localhost:5080/swagger/index.html' -UseBasicParsing -TimeoutSec 2
        if ($resp.StatusCode -eq 200) { $ready = $true; break }
    } catch {}
    Start-Sleep -Seconds 2
}
if (-not $ready) {
    Write-Warning "Backend didn't respond within 90s — check 'docker compose logs backend'. Continuing anyway."
}

Set-Location (Join-Path $root 'frontend')
if (-not (Test-Path node_modules)) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Cyan
    npm install
}

Write-Host "Starting frontend on http://localhost:4200 (Ctrl+C to stop)..." -ForegroundColor Cyan
npx ng serve --port 4200 --proxy-config proxy.conf.json
