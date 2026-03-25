#!/usr/bin/env bash
set -euo pipefail

# =========================
# Azure Networking Workshop
# Bootstrap for Cloud Shell
# Variant C: storage + demo blob URL
# Uses account key for blob data operations
# =========================

# ----- Variables -----
LOCATION="westeurope"
RG="rg-aznet-lab"

VNET_APP="vnet-app"
VNET_DATA="vnet-data"

APP_PREFIX="10.10.0.0/16"
DATA_PREFIX="10.20.0.0/16"

SNET_WEB="snet-web"
SNET_WEB_PREFIX="10.10.1.0/24"

SNET_PRIVATE="snet-private"
SNET_PRIVATE_PREFIX="10.10.2.0/24"

SNET_DATA="snet-data"
SNET_DATA_PREFIX="10.20.1.0/24"

NSG_WEB="nsg-web"
NSG_DATA="nsg-data"

# Storage account name must be globally unique, lowercase, 3-24 chars
RAND_SUFFIX=$(openssl rand -hex 3)
STORAGE_NAME="aznetlab${RAND_SUFFIX}"

# Blob demo objects
CONTAINER_NAME="demo"
BLOB_NAME="readme.txt"
LOCAL_FILE="readme.txt"

# Private DNS zone for Blob
PRIVATE_DNS_ZONE="privatelink.blob.core.windows.net"

echo "Using resource group: $RG"
echo "Using location: $LOCATION"
echo "Using storage account: $STORAGE_NAME"

echo
echo "Current Azure account:"
az account show --output table

# ----- Resource group -----
echo
echo "Creating resource group..."
az group create \
  --name "$RG" \
  --location "$LOCATION" \
  --output table

# ----- NSGs -----
echo
echo "Creating NSGs..."
az network nsg create \
  --resource-group "$RG" \
  --name "$NSG_WEB" \
  --location "$LOCATION" \
  --output table

az network nsg create \
  --resource-group "$RG" \
  --name "$NSG_DATA" \
  --location "$LOCATION" \
  --output table

# ----- VNets + subnets -----
echo
echo "Creating vnet-app and snet-web..."
az network vnet create \
  --resource-group "$RG" \
  --location "$LOCATION" \
  --name "$VNET_APP" \
  --address-prefixes "$APP_PREFIX" \
  --subnet-name "$SNET_WEB" \
  --subnet-prefixes "$SNET_WEB_PREFIX" \
  --output table

echo
echo "Creating snet-private..."
az network vnet subnet create \
  --resource-group "$RG" \
  --vnet-name "$VNET_APP" \
  --name "$SNET_PRIVATE" \
  --address-prefixes "$SNET_PRIVATE_PREFIX" \
  --output table

echo
echo "Creating vnet-data and snet-data..."
az network vnet create \
  --resource-group "$RG" \
  --location "$LOCATION" \
  --name "$VNET_DATA" \
  --address-prefixes "$DATA_PREFIX" \
  --subnet-name "$SNET_DATA" \
  --subnet-prefixes "$SNET_DATA_PREFIX" \
  --output table

# ----- Associate NSGs to subnets -----
echo
echo "Associating NSGs to subnets..."
az network vnet subnet update \
  --resource-group "$RG" \
  --vnet-name "$VNET_APP" \
  --name "$SNET_WEB" \
  --network-security-group "$NSG_WEB" \
  --output table

az network vnet subnet update \
  --resource-group "$RG" \
  --vnet-name "$VNET_DATA" \
  --name "$SNET_DATA" \
  --network-security-group "$NSG_DATA" \
  --output table

echo
echo "Subnet for private endpoints is ready: $SNET_PRIVATE"

# ----- Storage Account -----
echo
echo "Creating storage account..."
az storage account create \
  --resource-group "$RG" \
  --name "$STORAGE_NAME" \
  --location "$LOCATION" \
  --sku Standard_LRS \
  --kind StorageV2 \
  --min-tls-version TLS1_2 \
  --allow-blob-public-access true \
  --output table

# Blob service endpoint
BLOB_ENDPOINT=$(az storage account show \
  --resource-group "$RG" \
  --name "$STORAGE_NAME" \
  --query "primaryEndpoints.blob" \
  --output tsv)

# ----- Get account key for data operations -----
echo
echo "Getting storage account key for blob data operations..."
STORAGE_KEY=$(az storage account keys list \
  --resource-group "$RG" \
  --account-name "$STORAGE_NAME" \
  --query "[0].value" \
  --output tsv)

# ----- Demo file -----
echo
echo "Creating local demo file..."
cat > "$LOCAL_FILE" <<EOF
Azure Networking workshop demo blob

This file is here so you can demonstrate:
- standard blob service endpoint
- concrete blob URL
- later, the difference between public service access and Private Link

Storage account: $STORAGE_NAME
Resource group: $RG
EOF

# ----- Blob container -----
echo
echo "Creating blob container..."
az storage container create \
  --account-name "$STORAGE_NAME" \
  --account-key "$STORAGE_KEY" \
  --name "$CONTAINER_NAME" \
  --public-access blob \
  --output table

# ----- Upload demo blob -----
echo
echo "Uploading demo blob..."
az storage blob upload \
  --account-name "$STORAGE_NAME" \
  --account-key "$STORAGE_KEY" \
  --container-name "$CONTAINER_NAME" \
  --name "$BLOB_NAME" \
  --file "$LOCAL_FILE" \
  --overwrite true \
  --output table

BLOB_URL="${BLOB_ENDPOINT}${CONTAINER_NAME}/${BLOB_NAME}"

# ----- Private DNS zone -----
echo
echo "Creating private DNS zone for blob..."
az network private-dns zone create \
  --resource-group "$RG" \
  --name "$PRIVATE_DNS_ZONE" \
  --output table

echo
echo "Linking private DNS zone to vnet-app..."
az network private-dns link vnet create \
  --resource-group "$RG" \
  --zone-name "$PRIVATE_DNS_ZONE" \
  --name "link-${VNET_APP}-blob" \
  --virtual-network "$VNET_APP" \
  --registration-enabled false \
  --output table

# ----- Summary -----
echo
echo "Bootstrap complete."
echo "========================================"
echo "Resource group:        $RG"
echo "Location:              $LOCATION"
echo "VNet app:              $VNET_APP ($APP_PREFIX)"
echo "VNet data:             $VNET_DATA ($DATA_PREFIX)"
echo "Subnet web:            $SNET_WEB ($SNET_WEB_PREFIX)"
echo "Subnet private:        $SNET_PRIVATE ($SNET_PRIVATE_PREFIX)"
echo "Subnet data:           $SNET_DATA ($SNET_DATA_PREFIX)"
echo "NSG web:               $NSG_WEB"
echo "NSG data:              $NSG_DATA"
echo "Storage account:       $STORAGE_NAME"
echo "Blob service endpoint: $BLOB_ENDPOINT"
echo "Container:             $CONTAINER_NAME"
echo "Blob:                  $BLOB_NAME"
echo "Blob URL:              $BLOB_URL"
echo "Private DNS zone:      $PRIVATE_DNS_ZONE"
echo "========================================"
echo
echo "Next suggested workshop steps:"
echo "1) In Azure Portal create VNet peering between $VNET_APP and $VNET_DATA"
echo "2) In Azure Portal add NSG rules to $NSG_WEB and $NSG_DATA"
echo "3) In Azure Portal create a Private Endpoint for storage account $STORAGE_NAME into subnet $SNET_PRIVATE"
echo "4) Use the Blob URL above when explaining standard service endpoint vs Private Link path"