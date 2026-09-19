FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Calendary.slnx .
COPY src/Calendary.Api/Calendary.Api.csproj src/Calendary.Api/
COPY src/Calendary.Domain/Calendary.Domain.csproj src/Calendary.Domain/
COPY src/Calendary.Infrastructure/Calendary.Infrastructure.csproj src/Calendary.Infrastructure/
COPY src/Calendary.AI/Calendary.AI.csproj src/Calendary.AI/
RUN dotnet restore src/Calendary.Api/Calendary.Api.csproj

COPY src/ src/
RUN dotnet publish src/Calendary.Api/Calendary.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# restic powers the admin panel's read-only backup-status view (ResticBackupStatusService) — it
# reads the same repository deploy/backup.sh writes to, but never triggers a backup itself.
USER root
RUN apt-get update && apt-get install -y --no-install-recommends restic && rm -rf /var/lib/apt/lists/*

# Docker seeds a brand-new named volume from whatever's already at its mount path in the image, so
# pre-creating this directory owned by the non-root user below means any *fresh* media-data volume
# (new stack, restored-from-backup, recreated) starts writable — instead of silently inheriting
# root ownership and 403-ing every upload until someone chowns it by hand (see #352).
RUN mkdir -p /app/App_Data/media && chown -R $APP_UID /app/App_Data
USER $APP_UID

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Calendary.Api.dll"]
