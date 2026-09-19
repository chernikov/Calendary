#!/usr/bin/env bash
# Backs up a Calendary stack's MSSQL database and uploaded-photo/generated-sheet media volume into
# a restic repository on DigitalOcean Spaces. Runs on the droplet HOST, not inside any container —
# the mssql image ships no cron and no curl, so it cannot schedule or upload itself (see #305).
#
# Usage:
#   backup.sh                        # full backup (db + media) of both prod and staging
#   backup.sh --target prod          # full backup of one stack only
#   backup.sh --quick --target prod  # db-only, skips the media tar — fast pre-migration safety net
#
# Requires /opt/calendary/.env (prod) and /opt/calendary/.env.staging (staging) to define:
#   MSSQL_SA_PASSWORD, DO_SPACES_KEY, DO_SPACES_SECRET, DO_SPACES_BUCKET, DO_SPACES_REGION,
#   RESTIC_PASSWORD
# See deploy/RESTORE.md for how to restore from what this produces.
set -euo pipefail

CALENDARY_DIR="/opt/calendary"
QUICK=false
TARGET="all"

while [ $# -gt 0 ]; do
  case "$1" in
    --quick) QUICK=true; shift ;;
    --target) TARGET="$2"; shift 2 ;;
    *) echo "Unknown argument: $1" >&2; exit 1 ;;
  esac
done

WORKDIR="$(mktemp -d)"
trap 'rm -rf "$WORKDIR"' EXIT

backup_stack() {
  local stack="$1"            # prod | staging — used as a repo path segment and restic tag
  local project="$2"          # docker compose -p project name
  local compose_file="$3"
  local env_file="$4"
  local media_volume_key="$5" # volume key as declared in the compose file, e.g. media-data

  if [ ! -f "$env_file" ]; then
    echo "!! [$stack] $env_file not found, skipping" >&2
    return
  fi

  echo "== [$stack] loading $env_file =="
  set -a
  # shellcheck disable=SC1090
  source "$env_file"
  set +a

  if [ -z "${DO_SPACES_KEY:-}" ] || [ -z "${RESTIC_PASSWORD:-}" ]; then
    echo "!! [$stack] DO_SPACES_KEY/RESTIC_PASSWORD not configured, skipping backup" >&2
    return
  fi

  export AWS_ACCESS_KEY_ID="$DO_SPACES_KEY"
  export AWS_SECRET_ACCESS_KEY="$DO_SPACES_SECRET"
  export RESTIC_PASSWORD
  local repo="s3:https://${DO_SPACES_REGION}.digitaloceanspaces.com/${DO_SPACES_BUCKET}/calendary/${stack}"
  restic -r "$repo" snapshots >/dev/null 2>&1 || restic -r "$repo" init

  local ts bak_file mssql_container
  ts="$(date +%Y%m%d-%H%M%S)"
  bak_file="${stack}-${ts}.bak"

  echo "== [$stack] BACKUP DATABASE =="
  docker compose -p "$project" -f "$compose_file" exec -T mssql \
    /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
    -Q "BACKUP DATABASE [Calendary] TO DISK = N'/var/opt/mssql/data/${bak_file}' WITH FORMAT, INIT"
  mssql_container="$(docker compose -p "$project" -f "$compose_file" ps -q mssql)"
  docker cp "${mssql_container}:/var/opt/mssql/data/${bak_file}" "$WORKDIR/${bak_file}"
  docker compose -p "$project" -f "$compose_file" exec -T mssql rm "/var/opt/mssql/data/${bak_file}"

  echo "== [$stack] restic backup (db) =="
  restic -r "$repo" backup "$WORKDIR/${bak_file}" --tag db --host "calendary-${stack}"
  rm -f "$WORKDIR/${bak_file}"

  if [ "$QUICK" = false ]; then
    local media_volume media_file
    media_volume="$(docker volume ls \
      --filter "label=com.docker.compose.project=${project}" \
      --filter "label=com.docker.compose.volume=${media_volume_key}" \
      --format '{{.Name}}')"
    if [ -z "$media_volume" ]; then
      echo "!! [$stack] could not resolve media volume, skipping media backup" >&2
    else
      media_file="${stack}-media-${ts}.tar.gz"
      echo "== [$stack] archiving media volume ($media_volume) =="
      docker run --rm -v "${media_volume}:/media:ro" -v "${WORKDIR}:/out" alpine \
        tar czf "/out/${media_file}" -C /media .
      echo "== [$stack] restic backup (media) =="
      restic -r "$repo" backup "$WORKDIR/${media_file}" --tag media --host "calendary-${stack}"
      rm -f "$WORKDIR/${media_file}"
    fi
  fi

  echo "== [$stack] pruning (keep 7 daily, 4 weekly) =="
  restic -r "$repo" forget --keep-daily 7 --keep-weekly 4 --prune
}

cd "$CALENDARY_DIR"

if [ "$TARGET" = "all" ] || [ "$TARGET" = "prod" ]; then
  backup_stack prod calendary docker-compose.prod.yml "$CALENDARY_DIR/.env" media-data
fi

if [ "$TARGET" = "all" ] || [ "$TARGET" = "staging" ]; then
  backup_stack staging calendary-staging docker-compose.staging.yml "$CALENDARY_DIR/.env.staging" media-staging-data
fi

echo "Backup complete."
