# Restoring a Calendary stack from backup

Backups are produced daily by `deploy/backup.sh` (via the `calendary-backup.timer` systemd timer,
see `bootstrap.sh`) into a [restic](https://restic.net) repository on DigitalOcean Spaces — one
repo per stack, at `s3:https://<region>.digitaloceanspaces.com/<bucket>/calendary/<prod|staging>`.
Each daily run creates two restic snapshots tagged `db` and `media`.

This assumes you're restoring onto the same droplet, with the target stack (`docker compose ... up
-d`) already running — i.e. recovering from bad data or a failed migration, not rebuilding a
droplet from nothing. If the droplet itself is gone, re-run `bootstrap.sh` on a fresh one and a
`docker compose up -d` first, then follow the steps below.

## 1. Set up restic locally on the droplet

```bash
cd /opt/calendary
set -a; source .env; set +a          # or .env.staging for the staging stack
export AWS_ACCESS_KEY_ID="$DO_SPACES_KEY"
export AWS_SECRET_ACCESS_KEY="$DO_SPACES_SECRET"
export RESTIC_PASSWORD
REPO="s3:https://${DO_SPACES_REGION}.digitaloceanspaces.com/${DO_SPACES_BUCKET}/calendary/prod"  # or .../staging
```

## 2. Inspect what's available

```bash
restic -r "$REPO" snapshots
```

## 3. Restore the database

```bash
STACK=prod                            # or staging
PROJECT=calendary                     # or calendary-staging
COMPOSE_FILE=docker-compose.prod.yml  # or docker-compose.staging.yml

# Pull the latest db snapshot's .bak out to a local temp dir.
mkdir -p /tmp/restore && restic -r "$REPO" restore latest --tag db --target /tmp/restore
BAK_FILE=$(find /tmp/restore -name '*.bak' | sort | tail -1)

# Copy it into the running mssql container and restore over the existing database.
MSSQL_CONTAINER=$(docker compose -p "$PROJECT" -f "$COMPOSE_FILE" ps -q mssql)
docker cp "$BAK_FILE" "$MSSQL_CONTAINER:/var/opt/mssql/data/restore.bak"
docker compose -p "$PROJECT" -f "$COMPOSE_FILE" exec -T mssql \
  /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
  -Q "ALTER DATABASE [Calendary] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
      RESTORE DATABASE [Calendary] FROM DISK = N'/var/opt/mssql/data/restore.bak' WITH REPLACE;
      ALTER DATABASE [Calendary] SET MULTI_USER;"
docker compose -p "$PROJECT" -f "$COMPOSE_FILE" exec -T mssql rm /var/opt/mssql/data/restore.bak
```

If restoring onto a *different* SQL Server instance than the one the backup came from (e.g. a
rebuilt droplet with a fresh `mssql-data` volume) and the restore fails on logical file paths, add
`MOVE 'Calendary' TO '/var/opt/mssql/data/Calendary.mdf', MOVE 'Calendary_log' TO
'/var/opt/mssql/data/Calendary_log.ldf'` to the `RESTORE DATABASE` clause — check the backup's
actual logical file names first with `RESTORE FILELISTONLY FROM DISK = N'...'`.

## 4. Restore the media volume

```bash
# Stop the backend so nothing writes to the media volume mid-restore.
docker compose -p "$PROJECT" -f "$COMPOSE_FILE" stop backend

restic -r "$REPO" restore latest --tag media --target /tmp/restore-media
MEDIA_TAR=$(find /tmp/restore-media -name '*media*.tar.gz' | sort | tail -1)
MEDIA_VOLUME=$(docker volume ls \
  --filter "label=com.docker.compose.project=${PROJECT}" \
  --filter "label=com.docker.compose.volume=media-data" \
  --format '{{.Name}}')   # media-staging-data for staging

docker run --rm -v "${MEDIA_VOLUME}:/media" -v /tmp/restore-media:/in alpine \
  sh -c "rm -rf /media/* && tar xzf /in/$(basename "$MEDIA_TAR") -C /media"

docker compose -p "$PROJECT" -f "$COMPOSE_FILE" start backend
```

## 5. Clean up and verify

```bash
rm -rf /tmp/restore /tmp/restore-media
```

Log into the app and confirm orders/users are present and a known order's cover/sheet images load.

## Rehearsing this procedure

`.github/workflows/restore-staging.yml` (manual `workflow_dispatch` only, never runs
automatically) runs through this exact procedure against the **staging** stack, so it can be
exercised without any risk to production data.
