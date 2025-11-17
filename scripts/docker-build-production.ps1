#!/usr/bin/env pwsh
param(
    [Parameter()]
    [ValidateSet('backend', 'frontend', 'consumer', 'all')]
    [string]$Component = 'all',
    [Parameter()]
    [string]$Tag = 'latest',
    [Parameter()]
    [bool]$Push = $true,
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
Write-Info "  Calendary PRODUCTION Build"
Write-Info "========================================="
Write-Info "Component: $Component"
Write-Info "Tag: $Tag"
Write-Info "Push: $Push"

function Build-Backend {
    $backendImage = "chernikov/calendary.api:$Tag"
    $backendContext = Join-Path $rootDir "backend"
    $dockerfilePath = Join-Path $rootDir "backend\src\Calendary.Api\Dockerfile.production"
    
    if ($NoCache) {
        docker build --no-cache -t $backendImage -f $dockerfilePath $backendContext
    } else {
        docker build -t $backendImage -f $dockerfilePath $backendContext
    }
    
    if ($LASTEXITCODE -ne 0) { exit 1 }
    if ($Push) {
        docker push $backendImage
        if ($LASTEXITCODE -ne 0) { exit 1 }
    }
}

function Build-Consumer {
    $consumerImage = "chernikov/calendary.consumer:$Tag"
    $backendContext = Join-Path $rootDir "backend"
    $dockerfilePath = Join-Path $rootDir "backend\src\Calendary.Consumer\Dockerfile.production"
    
    if ($NoCache) {
        docker build --no-cache -t $consumerImage -f $dockerfilePath $backendContext
    } else {
        docker build -t $consumerImage -f $dockerfilePath $backendContext
    }
    
    if ($LASTEXITCODE -ne 0) { exit 1 }
    if ($Push) {
        docker push $consumerImage
        if ($LASTEXITCODE -ne 0) { exit 1 }
    }
}

function Build-Frontend {
    $frontendImage = "chernikov/calendary.ng:$Tag"
    $angularPath = Join-Path $rootDir "frontend\Calendary.Ng"
    $dockerfilePath = Join-Path $angularPath "Dockerfile.production"
    
    if ($NoCache) {
        docker build --no-cache -t $frontendImage -f $dockerfilePath $angularPath
    } else {
        docker build -t $frontendImage -f $dockerfilePath $angularPath
    }
    
    if ($LASTEXITCODE -ne 0) { exit 1 }
    if ($Push) {
        docker push $frontendImage
        if ($LASTEXITCODE -ne 0) { exit 1 }
    }
}
switch ($Component) {
    'backend' { Build-Backend }
    'frontend' { Build-Frontend }
    'consumer' { Build-Consumer }
    'all' { Build-Backend; Build-Consumer; Build-Frontend }
}
Write-Success "PRODUCTION Build completed!"
