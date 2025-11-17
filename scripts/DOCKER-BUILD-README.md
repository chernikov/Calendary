# Calendary Docker Build Scripts

## Overview

This directory contains scripts for building Docker images for the Calendary project.

## Files

- **docker-build.ps1** - PowerShell script for building Docker images
- **docker-build.bat** - Batch wrapper for Windows users

## Usage

### Build All Components (Backend + Frontend)

```bash
# PowerShell
.\scripts\docker-build.ps1

# Command Prompt
.\scripts\docker-build.bat
```

### Build Specific Component

```bash
# Backend only
.\scripts\docker-build.ps1 -Component backend

# Frontend only
.\scripts\docker-build.ps1 -Component frontend
```

### Build with Custom Tag

```bash
.\scripts\docker-build.ps1 -Tag v1.0.0
```

### Build and Push to Docker Hub

```bash
.\scripts\docker-build.ps1 -Tag v1.0.0 -Push
```

### Using Batch File

```cmd
REM Build all with default tag
docker-build.bat

REM Build backend with custom tag
docker-build.bat backend v1.0.0

REM Build and push
docker-build.bat all v1.0.0 push
```

## Docker Images

The script builds the following images:

1. **chernikov/calendary.api** - Backend API (.NET 9.0)
2. **chernikov/calendary.consumer** - RabbitMQ Consumer
3. **chernikov/calendary.ng** - Frontend (Angular + Nginx)

## Parameters

### PowerShell Script Parameters

- **Component** - Which component to build (backend, frontend, all)
  - Default: `all`
- **Tag** - Docker image tag
  - Default: `latest`
- **Push** - Switch to push images to Docker Hub after building
  - Default: `false`

### Batch File Arguments

1. Component (backend|frontend|all) - Default: `all`
2. Tag - Default: `latest`
3. Push (push) - Optional

## Examples

```powershell
# Development build
.\scripts\docker-build.ps1

# Production build with version tag
.\scripts\docker-build.ps1 -Tag v2.1.0 -Push

# Build only backend for testing
.\scripts\docker-build.ps1 -Component backend -Tag test

# Build frontend with custom tag
.\scripts\docker-build.ps1 -Component frontend -Tag dev-$(Get-Date -Format 'yyyyMMdd')
```

## Prerequisites

- Docker installed and running
- Docker Hub credentials configured (for push)
- PowerShell 5.1 or higher
- Node.js 20+ (for frontend build)
- .NET 9.0 SDK (for backend build)

## Docker Compose

After building images, you can use them with docker-compose:

```bash
docker-compose up -d
```

## Troubleshooting

### Build Fails

1. Check Docker is running: `docker ps`
2. Ensure you're in the project root directory
3. Check Docker Hub credentials: `docker login`

### Frontend Build Issues

1. Verify Angular project exists in `src/Calendary.Ng`
2. Check Node.js version: `node --version` (should be 20+)
3. Verify `package.json` exists

### Backend Build Issues

1. Verify .NET SDK is installed: `dotnet --version`
2. Check Dockerfile exists in project root
3. Ensure all NuGet packages are restored

## CI/CD Integration

The script can be integrated into CI/CD pipelines:

```yaml
# GitHub Actions example
- name: Build Docker Images
  run: |
    ./scripts/docker-build.ps1 -Tag ${{ github.sha }} -Push
```

```yaml
# Azure DevOps example
- pwsh: |
    ./scripts/docker-build.ps1 -Tag $(Build.BuildNumber) -Push
  displayName: 'Build and Push Docker Images'
```
