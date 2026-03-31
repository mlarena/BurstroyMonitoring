#!/bin/bash

# Settings for TCM service
APP_NAME="BurstroyMonitoring.TCM"
ZIP_FILE="BurstroyMonitoring.TCM.zip"
INSTALL_DIR="/opt/burstroy/tcm"
USER_NAME="burstroy"
SERVICE_NAME="burstroy-monitoring-tcm"

# Root check
if [ "$EUID" -ne 0 ]; then 
    echo "Please run as root: sudo $0"
    exit 1
fi

# Check if zip exists
if [ ! -f "$ZIP_FILE" ]; then
    echo "Error: $ZIP_FILE not found in current directory."
    exit 1
fi

echo "Updating $APP_NAME in $INSTALL_DIR (preserving config and snapshots)..."

# Stop service before update
if systemctl is-active --quiet $SERVICE_NAME; then
    echo "Stopping $SERVICE_NAME service..."
    systemctl stop $SERVICE_NAME
fi

# Create a temporary directory for extraction
TMP_EXTRACT="/tmp/burstroy_tcm_update"
rm -rf "$TMP_EXTRACT"
mkdir -p "$TMP_EXTRACT"

# Unzip to temp
echo "Extracting $ZIP_FILE to temporary folder..."
unzip -o "$ZIP_FILE" -d "$TMP_EXTRACT"

# Update all files (preserving config and snapshots)
echo "Updating files (preserving appsettings.json and snapshots)..."
# --delete: remove old files
# --exclude: keep config and snapshots
# IMPORTANT: trailing slash on source folder means "contents of this folder"
rsync -av --delete \
    --exclude='appsettings.json' \
    --exclude='appsettings.*.json' \
    --exclude='wwwroot/snapshots' \
    "$TMP_EXTRACT/" "$INSTALL_DIR/"

# Make executable
echo "Setting permissions..."
chmod +x "$INSTALL_DIR/$APP_NAME"
chown -R "$USER_NAME:$USER_NAME" "$INSTALL_DIR"

# Check file type
echo "File info for $APP_NAME:"
file "$INSTALL_DIR/$APP_NAME"

# Cleanup temp
rm -rf "$TMP_EXTRACT"

echo "✅ $APP_NAME updated successfully."
echo "You can now start the service: sudo systemctl start $SERVICE_NAME"
