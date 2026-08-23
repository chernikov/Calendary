#!/usr/bin/env bash
# One-time droplet setup. Run once as root on a fresh droplet:
#   ssh root@207.154.222.66 'bash -s' < deploy/bootstrap.sh
set -euo pipefail

echo "== Installing Docker Engine + Compose plugin =="
if ! command -v docker >/dev/null; then
  curl -fsSL https://get.docker.com | sh
fi
systemctl enable --now docker

echo "== Creating /opt/calendary =="
mkdir -p /opt/calendary
cd /opt/calendary

if [ ! -f .env ]; then
  cat > .env <<'EOF'
GHCR_OWNER=chernikov
DOMAIN=calendary.com.ua
MSSQL_SA_PASSWORD=CHANGE_ME_STRONG_PASSWORD
EOF
  echo ">>> Created /opt/calendary/.env with a placeholder password."
  echo ">>> Edit it now and set a real MSSQL_SA_PASSWORD before the first deploy:"
  echo ">>>   nano /opt/calendary/.env"
else
  echo "== /opt/calendary/.env already exists, leaving it as-is =="
fi

echo "== Opening firewall for SSH/HTTP/HTTPS (if ufw is active) =="
if command -v ufw >/dev/null && ufw status | grep -q "Status: active"; then
  ufw allow OpenSSH
  ufw allow 80/tcp
  ufw allow 443/tcp
fi

echo "== Done. =="
echo "Next: push to 'main' (or run the 'Deploy to production' workflow manually)"
echo "to build images and bring the stack up via GitHub Actions."
