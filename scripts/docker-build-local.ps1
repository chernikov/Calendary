#!/usr/bin/env pwsh
param(
    [Parameter()]
    [ValidateSet('backend', 'frontend', 'consumer', 'all')]
    [string]$Component = 'all',
    [Parameter()]
    [string]$Tag = 'dev',
    [Parameter()]
    [switch]$NoCache
)
$ErrorActionPreference = 'Stop'
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { Write-Host $args -ForegroundColor Red }
$rootDir = Split-Path -Parent $PSScriptRoot
Write-Info "========================================="
Write-Info "  Calendary LOCAL Build"
Write-Info "========================================="
$cacheFlag = if ($NoCache) { "--no-cache" } else { "" }
function Build-Backend {
    $backendImage = "calendary-api:$Tag"
    $dockerfilePath = Join-Path $rootDir "docker\backend\Dockerfile"
    $backendContext = Join-Path $rootDir "backend"
    docker build $cacheFlag -t $backendImage -f $dockerfilePath $backendContext
}
function Build-Consumer {
    $consumerImage = "calendary-consumer:$Tag"
    $dockerfilePath = Join-Path $rootDir "docker\backend\Dockerfile.consumer"
    $backendContext = Join-Path $rootDir "backend"
    docker build $cacheFlag -t $consumerImage -f $dockerfilePath $backendContext
}
function Build-Frontend {
    $frontendImage = "calendary-ng:$Tag"
    $angularPath = Join-Path $rootDir "frontend\Calendary.Ng"
    $nginxConf = Join-Path $rootDir "nginx.local.conf"
    $dockerfilePath = Join-Path $rootDir "docker\frontend\Dockerfile.local"
    $tempNginxPath = Join-Path $angularPath "nginx.local.conf"
    Copy-Item $nginxConf $tempNginxPath -Force
    docker build $cacheFlag -t $frontendImage -f $dockerfilePath $angularPath
    Remove-Item $tempNginxPath -Force -ErrorAction SilentlyContinue
}
switch ($Component) {
    'backend' { Build-Backend }
    'frontend' { Build-Frontend }
    'consumer' { Build-Consumer }
    'all' { Build-Backend; Build-Consumer; Build-Frontend }
}
Write-Success "Build completed!"
