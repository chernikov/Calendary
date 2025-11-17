#!/bin/bash
# Deployment Script for Calendary
# Generated: 2025-11-17
# Purpose: Deploy modern landing page + cart/checkout updates

set -e  # Exit on error

echo "🚀 Starting Calendary Deployment..."

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Step 1: Backend Database Migration
echo -e "${YELLOW}📦 Step 1: Running Database Migrations...${NC}"
cd src/Calendary.Api
dotnet ef database update --project ../Calendary.Repos
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Database migrations applied successfully${NC}"
else
    echo -e "${RED}❌ Database migration failed${NC}"
    exit 1
fi
cd ../..

# Step 2: Backend Build
echo -e "${YELLOW}🔨 Step 2: Building Backend...${NC}"
dotnet build Calendary.sln --configuration Release
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Backend build successful${NC}"
else
    echo -e "${RED}❌ Backend build failed${NC}"
    exit 1
fi

# Step 3: Frontend Dependencies
echo -e "${YELLOW}📦 Step 3: Installing Frontend Dependencies...${NC}"
cd src/Calendary.Ng
npm install
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Dependencies installed${NC}"
else
    echo -e "${RED}❌ npm install failed${NC}"
    exit 1
fi

# Step 4: Frontend Build (Production)
echo -e "${YELLOW}🔨 Step 4: Building Frontend (Production)...${NC}"
npm run build -- --configuration production
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Frontend build successful${NC}"
else
    echo -e "${RED}❌ Frontend build failed${NC}"
    exit 1
fi

# Step 5: SSR Build
echo -e "${YELLOW}🔨 Step 5: Building SSR...${NC}"
npm run build:ssr
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ SSR build successful${NC}"
else
    echo -e "${RED}❌ SSR build failed${NC}"
    exit 1
fi

cd ../..

# Step 6: Verify Build Outputs
echo -e "${YELLOW}🔍 Step 6: Verifying Build Outputs...${NC}"
if [ -d "src/Calendary.Ng/dist" ]; then
    echo -e "${GREEN}✅ dist/ folder exists${NC}"
    du -sh src/Calendary.Ng/dist
else
    echo -e "${RED}❌ dist/ folder not found${NC}"
    exit 1
fi

# Step 7: Deployment Summary
echo ""
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}   ✅ Deployment Build Complete!${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""
echo "📋 Deployment Checklist:"
echo "  [✓] Database migrations applied"
echo "  [✓] Backend built (Release mode)"
echo "  [✓] Frontend built (Production)"
echo "  [✓] SSR compiled"
echo ""
echo "📦 Next Steps:"
echo "  1. Test locally: cd src/Calendary.Ng && npm run serve:ssr"
echo "  2. Upload dist/ to production server"
echo "  3. Restart services on server"
echo "  4. Verify deployment: https://calendary.com.ua"
echo ""
echo "🎯 New Features Deployed:"
echo "  • Modern landing page (emoji placeholders)"
echo "  • Updated cart/checkout flow"
echo "  • UserPhoto functionality"
echo "  • Updated database schema (tracking/delivery cost)"
echo ""
echo -e "${YELLOW}⚠️  Manual Steps Required:${NC}"
echo "  1. Upload to server: scp -r dist/* user@server:/var/www/calendary-ng/"
echo "  2. SSH to server: ssh user@server"
echo "  3. Restart nginx: sudo systemctl restart nginx"
echo "  4. Restart API: docker-compose restart api"
echo ""
