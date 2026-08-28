#!/bin/bash
set -euo pipefail
APP="${1:?application name required}"
BASE=/opt/stackpulse
mkdir -p "$BASE/config"
aws ssm get-parameters-by-path --path "/stackpulse/prod/$APP/" --recursive --with-decryption --query 'Parameters[*].[Name,Value]' --output text |
while read -r name value; do printf '%s=%s\n' "${name##*/}" "$value"; done > "$BASE/config/$APP.env" || true
cd "$BASE/$APP"
docker compose up -d
