#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build Docker images for Calendary PRODUCTION deployment
.DESCRIPTION
    This script builds production-ready Docker images for calendary.com.ua with HTTPS support.
    It automatically uses production configurations and optimized builds.
.PARAMETER Component
    Specify which component to build: 'backend', 'frontend', 'consumer', or 'all' (default)
.PARAMETER Tag
    Docker image tag (default: 'latest')
.PARAMETER Push
    Push images to Docker Hub after building (default: true for production)
.EXAMPLE
    .\docker-build-production.ps1
    Build all components for production with 'latest' tag and push
.EXAMPLE
    .\docker-build-production.ps1 -Component frontend -Tag v1.0.0
    Build only frontend with custom tag
.EXAMPLE
    .\docker-build-production.ps1 -Tag v2.0.0 -Push:$false
    Build all without pushing
#>

param(
    [Parameter()]
    [ValidateSet('backend', 'frontend', 'consumer', 'all')]
    [string]$Component = 'all',
    
    [Parameter()]
    [string]$Tag = 'latest',
    
    [Parameter()]
    [switch]$Push = $true
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
Write-Info "  Calendary PRODUCTION Build"
Write-Info "========================================="
Write-Info "Component: $Component"
Write-Info "Tag: $Tag"
Write-Info "Push to Docker Hub: $Push"
Write-Info "Target: calendary.com.ua"
Write-Info "Root Directory: $rootDir"
Write-Info "========================================="
Write-Warning "`nBuilding PRODUCTION images with optimizations and HTTPS support"
Write-Info "========================================="

# Function to build backend for production
function Build-Backend {
    Write-Info "`n[Backend API] Building PRODUCTION Docker image..."
    
    $backendImage = "chernikov/calendary.api:$Tag"
    
    try {
        Write-Info "[Backend API] Building optimized image: $backendImage"
        $dockerfilePath = Join-Path $rootDir "docker\backend\Dockerfile.production"
        docker build -t $backendImage -f $dockerfilePath $rootDir
        
        if ($LASTEXITCODE -ne 0) {
            throw "Backend build failed"
        }
        
        Write-Success "[Backend API] ✓ Production image built successfully: $backendImage"
        
        if ($Push) {
            Write-Info "[Backend API] Pushing to Docker Hub..."
            docker push $backendImage
            Write-Success "[Backend API] ✓ Image pushed successfully"
        }
    }
    catch {
        Write-Error "[Backend API] ✗ Build failed: $_"
        exit 1
    }
}

# Function to build consumer for production
function Build-Consumer {
    Write-Info "`n[Consumer] Building PRODUCTION Docker image..."
    
    $consumerImage = "chernikov/calendary.consumer:$Tag"
    
    try {
        Write-Info "[Consumer] Building optimized image: $consumerImage"
        $dockerfilePath = Join-Path $rootDir "docker\backend\Dockerfile.consumer"
        docker build -t $consumerImage -f $dockerfilePath $rootDir
        
        if ($LASTEXITCODE -ne 0) {
            throw "Consumer build failed"
        }
        
        Write-Success "[Consumer] ✓ Production image built successfully: $consumerImage"
        
        if ($Push) {
            Write-Info "[Consumer] Pushing to Docker Hub..."
            docker push $consumerImage
            Write-Success "[Consumer] ✓ Image pushed successfully"
        }
    }
    catch {
        Write-Error "[Consumer] ✗ Build failed: $_"
        exit 1
    }
}

# Function to build frontend for production
function Build-Frontend {
    Write-Info "`n[Frontend] Building PRODUCTION Docker image with HTTPS..."
    
    $frontendImage = "chernikov/calendary.ng:$Tag"
    
    try {
        # Check if Angular project exists
        $angularPath = Join-Path $rootDir "src\Calendary.Ng"
        if (-not (Test-Path $angularPath)) {
            throw "Angular project not found at: $angularPath"
        }
        
        $dockerfilePath = Join-Path $rootDir "docker\frontend\Dockerfile.production"
        
        Write-Info "[Frontend] Building production image with SSL support: $frontendImage"
        Write-Info "[Frontend] Configuration: calendary.com.ua with HTTPS redirect"
        
        # Build from src/Calendary.Ng directory
        docker build -t $frontendImage -f $dockerfilePath $angularPath
        
        if ($LASTEXITCODE -ne 0) {
            throw "Frontend build failed"
        }
        
        Write-Success "[Frontend] ✓ Production image built successfully: $frontendImage"
        Write-Success "[Frontend] ✓ HTTPS ready for calendary.com.ua"
        
        if ($Push) {
            Write-Info "[Frontend] Pushing to Docker Hub..."
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
    $startTime = Get-Date
    
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
    
    $duration = (Get-Date) - $startTime
    
    Write-Success "`n========================================="
    Write-Success "  PRODUCTION Build Completed! ✓"
    Write-Success "========================================="
    Write-Info "Build time: $($duration.ToString('mm\:ss'))"
    
    # Show built images
    Write-Info "`nProduction images:"
    docker images | Select-String "chernikov/calendary"
    
    Write-Info "`n========================================="
    Write-Info "Next steps:"
    Write-Info "1. Verify images: docker images | grep chernikov/calendary"
    Write-Info "2. Test locally: docker-compose up -d"
    Write-Info "3. Deploy to production server"
    Write-Info "4. Verify SSL certificates in /certs directory"
    Write-Info "========================================="
}
catch {
    Write-Error "`n========================================="
    Write-Error "  PRODUCTION Build Failed! ✗"
    Write-Error "========================================="
    Write-Error $_.Exception.Message
    exit 1
}
