#!/usr/bin/env bash
set -euo pipefail

RG="rg-aznet-lab"
VNET_APP="vnet-app"
VNET_DATA="vnet-data"
NSG_WEB="nsg-web"
NSG_DATA="nsg-data"
CONTAINER_NAME="demo"

echo "Checking resource group..."
az group show --name "$RG" --output table

echo
echo "Checking VNets..."
az network vnet list \
  --resource-group "$RG" \
  --output table

echo
echo "Checking subnets in vnet-app..."
az network vnet subnet list \
  --resource-group "$RG" \
  --vnet-name "$VNET_APP" \
  --output table

echo
echo "Checking subnets in vnet-data..."
az network vnet subnet list \
  --resource-group "$RG" \
  --vnet-name "$VNET_DATA" \
  --output table

echo
echo "Checking NSGs..."
az network nsg list \
  --resource-group "$RG" \
  --output table

echo
echo "Checking NSG rules (including defaults) for nsg-web..."
az network nsg rule list \
  --resource-group "$RG" \
  --nsg-name "$NSG_WEB" \
  --include-default \
  --output table

echo
echo "Checking NSG rules (including defaults) for nsg-data..."
az network nsg rule list \
  --resource-group "$RG" \
  --nsg-name "$NSG_DATA" \
  --include-default \
  --output table

echo
echo "Checking VNet peerings..."
az network vnet peering list \
  --resource-group "$RG" \
  --vnet-name "$VNET_APP" \
  --output table || true

az network vnet peering list \
  --resource-group "$RG" \
  --vnet-name "$VNET_DATA" \
  --output table || true

echo
echo "Checking private endpoints..."
az network private-endpoint list \
  --resource-group "$RG" \
  --output table || true

echo
echo "Checking private DNS zones..."
az network private-dns zone list \
  --resource-group "$RG" \
  --output table || true

echo
echo "Checking storage account..."
STORAGE_NAME=$(az storage account list \
  --resource-group "$RG" \
  --query '[0].name' \
  --output tsv)

if [ -z "${STORAGE_NAME:-}" ]; then
  echo "No storage account found in $RG"
  exit 1
fi

echo "Storage account: $STORAGE_NAME"

BLOB_ENDPOINT=$(az storage account show \
  --resource-group "$RG" \
  --name "$STORAGE_NAME" \
  --query "primaryEndpoints.blob" \
  --output tsv)

echo "Blob service endpoint: $BLOB_ENDPOINT"

STORAGE_KEY=$(az storage account keys list \
  --resource-group "$RG" \
  --account-name "$STORAGE_NAME" \
  --query "[0].value" \
  --output tsv)

echo
echo "Checking blob containers..."
az storage container list \
  --account-name "$STORAGE_NAME" \
  --account-key "$STORAGE_KEY" \
  --output table

echo
echo "Checking blobs in container '$CONTAINER_NAME'..."
az storage blob list \
  --account-name "$STORAGE_NAME" \
  --account-key "$STORAGE_KEY" \
  --container-name "$CONTAINER_NAME" \
  --output table || true

echo
echo "Checking blob URL..."
BLOB_URL="${BLOB_ENDPOINT}${CONTAINER_NAME}/readme.txt"
echo "$BLOB_URL"

echo
echo "========================================"
echo "DNS resolution check"
echo "========================================"
STORAGE_HOST=$(echo "$BLOB_ENDPOINT" | sed 's|https://||' | sed 's|/$||')
echo "Resolving: $STORAGE_HOST"
nslookup "$STORAGE_HOST" || true
echo
echo "Tip: spustte tento skript znovu po vytvoreni Private Endpointu."
echo "     Pokud DNS preklad vraci privatni IP (10.x.x.x), Private Link funguje."