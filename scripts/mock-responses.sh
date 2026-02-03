#!/bin/bash
set -euo pipefail

PORT="${1:-${MASTER_PORT_NAME:-/tmp/ttyV1}}"
REQ_ID="${2:-$(uuidgen | tr -d '-')}"

printf "ACK,%s\n" "$REQ_ID" > "$PORT"
printf "STATUS_PILOT,%s,00:00:01.000,1,2,3\n" "$REQ_ID" > "$PORT"
printf "STATUS_ANCHOR,%s,00:00:01.000,1,1,1,1\n" "$REQ_ID" > "$PORT"
printf "STATUS_ANCHOR,%s,00:00:01.000,2,2,2,2\n" "$REQ_ID" > "$PORT"
printf "STATUS_ANCHOR,%s,00:00:01.000,3,3,3,3\n" "$REQ_ID" > "$PORT"
