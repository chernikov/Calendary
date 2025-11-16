# Epic 02 - Customer Portal: Setup Instructions

This document provides setup instructions for the Customer Portal backend (Tasks 04-08).

## Overview

The following tasks have been completed:

- **Task 07**: Database schema for Customer Portal entities
- **Task 04**: API endpoints for calendar templates
- **Task 05**: API for managing user calendars
- **Task 06**: File upload service for images
- **Task 08**: CI/CD for React frontend

## Database Migration

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server (local or remote)
- Entity Framework Core tools

### Steps to Apply Migration

1. **Navigate to the Repos project**:
   ```bash
   cd src/Calendary.Repos
   ```

2. **Create the migration** (if not already created):
   ```bash
   dotnet ef migrations add AddCustomerPortalTables --startup-project ../Calendary.Api/Calendary.Api.csproj
   ```

3. **Apply the migration to the database**:
   ```bash
   dotnet ef database update --startup-project ../Calendary.Api/Calendary.Api.csproj
   ```

### New Database Tables

The migration creates the following tables:

1. **Templates** - Calendar templates catalog
   - Id, Name, Description, Category
   - PreviewImageUrl, TemplateData (JSON)
   - Price, IsActive, SortOrder
   - CreatedAt, UpdatedAt

2. **UserCalendars** - User-created calendars
   - Id, UserId, TemplateId (nullable)
   - Title, DesignData (JSON), PreviewImageUrl
   - Status (Draft/Completed), IsDeleted
   - CreatedAt, UpdatedAt

3. **CartItems** - Shopping cart items
   - Id, UserId, CalendarId
   - Quantity, Format (A3/A4), PaperType (Glossy/Matte)
   - Price, CreatedAt, UpdatedAt

4. **UploadedFiles** - User-uploaded images
   - Id, UserId, FileName, FileSize, MimeType
   - Url, ThumbnailUrl
   - CreatedAt, IsDeleted

### Seed Data

The migration includes 10 template calendars:
- Family Calendar 2026
- Corporate Calendar 2026
- Sports Calendar 2026
- Wedding Calendar 2026
- Kids Calendar 2026
- Minimalist Calendar 2026
- Nature 2026
- Travel 2026
- Vintage 2026
- Professional 2026

## API Endpoints

### Templates API

**Base URL**: `/api/templates`

- `GET /api/templates` - Get paginated templates
  - Query params: `category`, `search`, `page`, `pageSize`, `sortBy`
  - Returns: `PagedResult<TemplateDto>`

- `GET /api/templates/{id}` - Get template by ID
  - Returns: `TemplateDetailDto`

- `GET /api/templates/categories` - Get all categories
  - Returns: `string[]`

- `GET /api/templates/featured` - Get featured templates
  - Query params: `count` (default: 5)
  - Returns: `TemplateDto[]`

### Calendars API

**Base URL**: `/api/calendars`
**Authorization**: Required (JWT)

- `GET /api/calendars` - Get user's calendars
  - Returns: `UserCalendarDto[]`

- `GET /api/calendars/{id}` - Get calendar by ID
  - Returns: `UserCalendarDetailDto`

- `POST /api/calendars` - Create new calendar
  - Body: `CreateCalendarRequest`
  - Returns: `UserCalendarDetailDto`

- `PUT /api/calendars/{id}` - Update calendar
  - Body: `UpdateCalendarRequest`
  - Returns: `UserCalendarDetailDto`

- `DELETE /api/calendars/{id}` - Delete calendar (soft delete)
  - Returns: 204 No Content

- `POST /api/calendars/{id}/duplicate` - Duplicate calendar
  - Returns: `UserCalendarDetailDto`

### Files API

**Base URL**: `/api/files`
**Authorization**: Required (JWT)

- `POST /api/files/upload` - Upload single file
  - Body: `IFormFile` (multipart/form-data)
  - Max size: 10MB
  - Allowed formats: JPG, PNG, WEBP, GIF
  - Returns: `FileUploadResponse`

- `POST /api/files/upload/multiple` - Upload multiple files
  - Body: `IFormFileCollection` (multipart/form-data)
  - Max files: 10
  - Max total size: 50MB
  - Returns: `FileUploadResponse[]`

- `DELETE /api/files/{id}` - Delete file
  - Returns: 204 No Content

- `GET /api/files/{id}` - Get file URL
  - Returns: `{ url: string }`

## File Upload Configuration

### Upload Directory Setup

1. **Create uploads directory**:
   ```bash
   mkdir -p src/Calendary.Api/wwwroot/uploads
   ```

2. **Set appropriate permissions** (Linux/Mac):
   ```bash
   chmod 755 src/Calendary.Api/wwwroot/uploads
   ```

### Configuration (appsettings.json)

Add to `src/Calendary.Api/appsettings.json`:

```json
{
  "FileStorage": {
    "MaxFileSize": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp", ".gif"],
    "UploadPath": "wwwroot/uploads"
  }
}
```

## Frontend CI/CD Setup

### GitHub Actions Workflows

Two workflows have been created:

1. **frontend-ci.yml** - Runs on PRs and pushes
   - Linting (ESLint)
   - Formatting check (Prettier)
   - Type checking (TypeScript)
   - Tests
   - Build

2. **frontend-cd.yml** - Deploys to production
   - Runs on main branch push
   - Deploys to Vercel

### Vercel Setup

1. **Install Vercel CLI**:
   ```bash
   npm i -g vercel
   ```

2. **Link project to Vercel**:
   ```bash
   cd calendary-customer-portal
   vercel link
   ```

3. **Configure GitHub Secrets**:
   - `VERCEL_TOKEN` - Vercel authentication token
   - `VERCEL_ORG_ID` - Organization ID
   - `VERCEL_PROJECT_ID` - Project ID
   - `NEXT_PUBLIC_API_URL` - Backend API URL

### Alternative Deployment (Azure/AWS)

If not using Vercel, update `.github/workflows/frontend-cd.yml` for:
- Azure Static Web Apps
- AWS S3 + CloudFront
- Netlify
- Other hosting providers

## Testing the Implementation

### 1. Test Templates API

```bash
# Get all templates
curl http://localhost:5000/api/templates

# Get templates by category
curl http://localhost:5000/api/templates?category=Сімейний

# Search templates
curl http://localhost:5000/api/templates?search=2026

# Get featured templates
curl http://localhost:5000/api/templates/featured
```

### 2. Test Calendars API (requires authentication)

```bash
# Login and get token
TOKEN=$(curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}' \
  | jq -r '.token')

# Create calendar
curl -X POST http://localhost:5000/api/calendars \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"My Calendar 2026","templateId":1}'

# Get user calendars
curl http://localhost:5000/api/calendars \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Test File Upload

```bash
# Upload file
curl -X POST http://localhost:5000/api/files/upload \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@/path/to/image.jpg"
```

## Dependencies Registered

### Repositories (Calendary.Repos)

- `ITemplateRepository` → `TemplateRepository`
- `IUserCalendarRepository` → `UserCalendarRepository`
- `IUploadedFileRepository` → `UploadedFileRepository`

### Services (Calendary.Core)

- `IFileUploadService` → `FileUploadService`

All dependencies are automatically registered via:
- `services.AddCalendaryRepositories(connectionString)`
- `services.AddCoreServices(configuration)`

## Known Limitations & TODOs

### File Upload Service

The current implementation saves files as-is. For production, implement:

1. **Image Processing with ImageSharp**:
   ```bash
   dotnet add package SixLabors.ImageSharp
   ```

2. **Features to add**:
   - Resize images to max 4000x4000
   - Generate thumbnails (300x300)
   - Optimize quality (JPEG quality 90%)
   - Convert to WebP for web
   - Add watermarks (optional)

3. **Cloud Storage Integration**:
   - Azure Blob Storage
   - AWS S3
   - Google Cloud Storage
   - CDN integration

### Security Enhancements

1. **Rate Limiting**:
   ```bash
   dotnet add package AspNetCoreRateLimit
   ```

2. **Virus Scanning** (optional):
   - Integrate ClamAV
   - Use cloud virus scanning service

3. **Image Validation**:
   - Verify file content matches extension
   - Check for malicious content

## Troubleshooting

### Migration Fails

```bash
# Reset migrations
dotnet ef database drop
dotnet ef database update

# Check connection string
dotnet ef dbcontext info
```

### File Upload Fails

1. Check directory permissions
2. Verify file size limits in web.config/program.cs
3. Check allowed MIME types
4. Review server logs

### API Returns 404

1. Verify controllers are registered
2. Check routing configuration
3. Ensure repositories are registered in DI
4. Review middleware order in Program.cs

## Next Steps

1. **Create Frontend** (Task 01):
   - Set up Next.js project in `calendary-customer-portal/`
   - Install dependencies
   - Configure API client

2. **Implement UI Components** (Tasks 09-16):
   - Template catalog page
   - Calendar editor
   - Shopping cart
   - Checkout flow

3. **Add Authentication Flow** (Task 02):
   - Login/Register pages
   - JWT token handling
   - Protected routes

## Support

For issues or questions:
- Check existing GitHub issues
- Review API documentation in Swagger (http://localhost:5000/swagger)
- Review server logs in `logs/` directory

---

**Created**: 2025-11-16
**Epic**: 02 - Customer Portal
**Tasks**: 04, 05, 06, 07, 08
