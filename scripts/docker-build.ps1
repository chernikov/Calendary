#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build Docker images for Calendary project (Backend API and Frontend Angular)
.DESCRIPTION
    This script builds Docker images for both backend (.NET API) and frontend (Angular) applications.
    It supports building individual components or all at once.
.PARAMETER Component
    Specify which component to build: 'backend', 'frontend', 'consumer', or 'all' (default)
.PARAMETER Tag
    Docker image tag (default: 'latest')
.PARAMETER Push
    Push images to Docker Hub after building
.EXAMPLE
    .\docker-build.ps1
    Build both backend and frontend with 'latest' tag
.EXAMPLE
    .\docker-build.ps1 -Component backend -Tag v1.0.0
    Build only backend with custom tag
.EXAMPLE
    .\docker-build.ps1 -Tag v1.0.0 -Push
    Build and push both images with custom tag
#>

param(
    [Parameter()]
    [ValidateSet('backend', 'frontend', 'consumer', 'all')]
    [string]$Component = 'all',
    
    [Parameter()]
    [string]$Tag = 'latest',
    
    [Parameter()]
    [switch]$Push,
    
    [Parameter()]
    [switch]$Production
)

$ErrorActionPreference = 'Stop'

# Colors for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { Write-Host $args -ForegroundColor Red }

# Get script directory
$rootDir = Split-Path -Parent $PSScriptRoot

Write-Info "========================================="
Write-Info "  Calendary Docker Build Script"
Write-Info "========================================="
Write-Info "Component: $Component"
Write-Info "Tag: $Tag"
Write-Info "Environment: $(if ($Production) { 'PRODUCTION' } else { 'LOCAL' })"
Write-Info "Root Directory: $rootDir"
Write-Info "========================================="

# Function to build backend
function Build-Backend {
    Write-Info "`n[Backend] Building Docker image..."
    
    $backendImage = "chernikov/calendary.api:$Tag"
    
    try {
        # Build backend API
        Write-Info "[Backend] Building API image: $backendImage"
        $dockerfilePath = Join-Path $rootDir "docker\backend\Dockerfile"
        docker build -t $backendImage -f $dockerfilePath $rootDir
        
        if ($LASTEXITCODE -ne 0) {
            throw "Backend build failed"
        }
        
        Write-Success "[Backend] ✓ API image built successfully: $backendImage"
        
        if ($Push) {
            Write-Info "[Backend] Pushing image to Docker Hub..."
            docker push $backendImage
            Write-Success "[Backend] ✓ Image pushed successfully"
        }
    }
    catch {
        Write-Error "[Backend] ✗ Build failed: $_"
        exit 1
    }
}

# Function to build consumer
function Build-Consumer {
    Write-Info "`n[Consumer] Building Docker image..."
    
    $consumerImage = "chernikov/calendary.consumer:$Tag"
    
    try {
        Write-Info "[Consumer] Building Consumer image: $consumerImage"
        $dockerfilePath = Join-Path $rootDir "docker\backend\Dockerfile.consumer"
        docker build -t $consumerImage -f $dockerfilePath $rootDir
        
        if ($LASTEXITCODE -ne 0) {
            throw "Consumer build failed"
        }
        
        Write-Success "[Consumer] ✓ Consumer image built successfully: $consumerImage"
        
        if ($Push) {
            Write-Info "[Consumer] Pushing image to Docker Hub..."
            docker push $consumerImage
            Write-Success "[Consumer] ✓ Image pushed successfully"
        }
    }
    catch {
        Write-Error "[Consumer] ✗ Build failed: $_"
        exit 1
    }
}

# Function to build frontend
function Build-Frontend {
    Write-Info "`n[Frontend] Building Docker image..."
    
    $frontendImage = "chernikov/calendary.ng:$Tag"
    
    try {
        # Check if Angular project exists
        $angularPath = Join-Path $rootDir "src\Calendary.Ng"
        if (-not (Test-Path $angularPath)) {
            throw "Angular project not found at: $angularPath"
        }
        
        # Select Dockerfile based on environment
        if ($Production) {
            $dockerfilePath = Join-Path $rootDir "docker\frontend\Dockerfile.production"
            Write-Info "[Frontend] Building PRODUCTION image: $frontendImage"
        } else {
            $dockerfilePath = Join-Path $rootDir "docker\frontend\Dockerfile.local"
            Write-Info "[Frontend] Building LOCAL image: $frontendImage"
        }
        
        # Build from src/Calendary.Ng directory
        docker build -t $frontendImage -f $dockerfilePath $angularPath
        
        if ($LASTEXITCODE -ne 0) {
            throw "Frontend build failed"
        }
        
        Write-Success "[Frontend] ✓ Image built successfully: $frontendImage"
        
        if ($Push) {
            Write-Info "[Frontend] Pushing image to Docker Hub..."
            docker push $frontendImage
            Write-Success "[Frontend] ✓ Image pushed successfully"
        }
    }
    catch {
        Write-Error "[Frontend] ✗ Build failed: $_"
        exit 1
    }
}

# Main execution
try {
    switch ($Component) {
        'backend' {
            Build-Backend
        }
        'frontend' {
            Build-Frontend
        }
        'consumer' {
            Build-Consumer
        }
        'all' {
            Build-Backend
            Build-Consumer
            Build-Frontend
        }
    }
    
    Write-Success "`n========================================="
    Write-Success "  Build completed successfully! ✓"
    Write-Success "========================================="
    
    # Show built images
    Write-Info "`nBuilt images:"
    docker images | Select-String "chernikov/calendary"
}
catch {
    Write-Error "`n========================================="
    Write-Error "  Build failed! ✗"
    Write-Error "========================================="
    Write-Error $_.Exception.Message
    exit 1
}
