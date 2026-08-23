#!/usr/bin/env bash
# One-time droplet setup. Run once as root on a fresh droplet:
#   ssh root@207.154.222.66 'bash -s' < deploy/bootstrap.sh
set -euo pipefail

echo "== Installing Docker Engine + Compose plugin =="
if ! command -v docker >/dev/null; then
  curl -fsSL https://get.docker.com | sh
fi
systemctl enable --now docker

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
MSSQL_SA_PASSWORD=CHANGE_ME_DIFFERENT_STRONG_PASSWORD
EOF
  echo ">>> Created /opt/calendary/.env.staging with a placeholder password — edit before deploying staging."
else
  echo "== /opt/calendary/.env.staging already exists, leaving it as-is =="
fi

echo "== Opening firewall for SSH/HTTP/HTTPS (if ufw is active) =="
if command -v ufw >/dev/null && ufw status | grep -q "Status: active"; then
  ufw allow OpenSSH
  ufw allow 80/tcp
  ufw allow 443/tcp
fi

echo "== Done. =="
echo "Next: push to 'main' to deploy prod + edge, or 'develop' to deploy staging,"
echo "via the GitHub Actions workflows."
