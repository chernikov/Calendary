#!/usr/bin/env bash
# One-time droplet setup. Run once as root on a fresh droplet:
#   ssh root@207.154.222.66 'bash -s' < deploy/bootstrap.sh
set -euo pipefail

echo "== Installing Docker Engine + Compose plugin =="
if ! command -v docker >/dev/null; then
  curl -fsSL https://get.docker.com | sh
fi
systemctl enable --now docker

echo "== Installing restic (off-droplet backups, see #305) =="
if ! command -v restic >/dev/null; then
  apt-get update -qq && apt-get install -y -qq restic
fi

echo "== Creating shared 'web' network (fronted by edge Caddy) =="
docker network inspect web >/dev/null 2>&1 || docker network create web

echo "== Creating /opt/calendary =="
mkdir -p /opt/calendary
cd /opt/calendary

if [ ! -f .env ]; then
  cat > .env <<'EOF'
GHCR_OWNER=chernikov
DOMAIN=calendary.com.ua
STAGING_DOMAIN=staging.calendary.com.ua
MSSQL_SA_PASSWORD=CHANGE_ME_STRONG_PASSWORD
EOF
  echo ">>> Created /opt/calendary/.env with a placeholder password."
  echo ">>> Edit it now and set a real MSSQL_SA_PASSWORD before the first deploy:"
  echo ">>>   nano /opt/calendary/.env"
else
  echo "== /opt/calendary/.env already exists, leaving it as-is =="
fi

if [ ! -f .env.staging ]; then
  cat > .env.staging <<'EOF'
GHCR_OWNER=chernikov
STAGING_DOMAIN=staging.calendary.com.ua
MSSQL_SA_PASSWORD=CHANGE_ME_DIFFERENT_STRONG_PASSWORD
EOF
  echo ">>> Created /opt/calendary/.env.staging with a placeholder password — edit before deploying staging."
else
  echo "== /opt/calendary/.env.staging already exists, leaving it as-is =="
fi

echo "== Adding STAGING_DOMAIN to .env.staging (if missing) =="
# Idempotent, same shape as the backup-config block below — a droplet bootstrapped before this was
# added would otherwise never get it, and docker-compose.staging.yml's Cors__AllowedOrigins__0/
# Monobank__PublicBaseUrl silently resolve to a bare "https://" without it (see #384: this exact
# gap broke Monobank's webhook/redirect URLs on staging).
grep -q '^STAGING_DOMAIN=' .env.staging 2>/dev/null || printf 'STAGING_DOMAIN=staging.calendary.com.ua\n' >> .env.staging

echo "== Adding backup config placeholders to .env/.env.staging (if missing) =="
# Idempotent, unlike the two blocks above — .env/.env.staging already exist on a droplet that's
# been through bootstrap before, so a fresh-file-only check would silently skip these forever.
for env_file in .env .env.staging; do
  grep -q '^DO_SPACES_KEY=' "$env_file" 2>/dev/null || cat >> "$env_file" <<'EOF'

# Off-droplet backups (see deploy/backup.sh, deploy/RESTORE.md, issue #305).
# DO_SPACES_KEY=
# DO_SPACES_SECRET=
# DO_SPACES_BUCKET=
# DO_SPACES_REGION=
# RESTIC_PASSWORD=
EOF
done
echo ">>> Fill in DO_SPACES_*/RESTIC_PASSWORD in .env and .env.staging before backups can run."
echo ">>> Save RESTIC_PASSWORD somewhere OTHER than this droplet — losing it makes every backup"
echo ">>> permanently undecryptable."

echo "== Installing the daily backup systemd timer =="
cat > /etc/systemd/system/calendary-backup.service <<'EOF'
[Unit]
Description=Calendary off-droplet backup (DB + media)

[Service]
Type=oneshot
WorkingDirectory=/opt/calendary
ExecStart=/opt/calendary/backup.sh
EOF
cat > /etc/systemd/system/calendary-backup.timer <<'EOF'
[Unit]
Description=Run Calendary backups daily

[Timer]
OnCalendar=daily
Persistent=true

[Install]
WantedBy=timers.target
EOF
systemctl daemon-reload
systemctl enable --now calendary-backup.timer

echo "== Opening firewall for SSH/HTTP/HTTPS (if ufw is active) =="
if command -v ufw >/dev/null && ufw status | grep -q "Status: active"; then
  ufw allow OpenSSH
  ufw allow 80/tcp
  ufw allow 443/tcp
fi

echo "== Done. =="
echo "Next: push to 'main' to deploy prod + edge, or 'develop' to deploy staging,"
echo "via the GitHub Actions workflows."
