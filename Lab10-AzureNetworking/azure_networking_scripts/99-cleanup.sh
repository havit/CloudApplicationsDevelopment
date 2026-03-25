#!/usr/bin/env bash
set -euo pipefail

RG="rg-aznet-lab"

echo "Deleting resource group: $RG"
az group delete \
  --name "$RG" \
  --yes \
  --no-wait

echo "Delete started."
