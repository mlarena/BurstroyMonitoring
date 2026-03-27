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

# Check if zip exists
if [ ! -f "$ZIP_FILE" ]; then
    echo "Error: $ZIP_FILE not found in current directory."
    exit 1
fi

echo "Updating $APP_NAME in $INSTALL_DIR (preserving config and snapshots)..."

# Stop service before update
if systemctl is-active --quiet  burstroy-monitoring-tcm; then
    echo "Stopping  burstroy-monitoring-tcm service..."
    systemctl stop  burstroy-monitoring-tcm
fi

# Create a temporary directory for extraction
TMP_EXTRACT="/tmp/burstroy_tcm_update"
rm -rf "$TMP_EXTRACT"
mkdir -p "$TMP_EXTRACT"

# Unzip to temp
echo "Extracting $ZIP_FILE to temporary folder..."
unzip -o "$ZIP_FILE" -d "$TMP_EXTRACT"

# 1. Update executable
echo "Updating executable..."
cp "$TMP_EXTRACT/$APP_NAME" "$INSTALL_DIR/"

# 2. Update wwwroot (preserving snapshots)
echo "Updating wwwroot (preserving snapshots)..."
if [ -d "$TMP_EXTRACT/wwwroot" ]; then
    # Create wwwroot if not exists
    mkdir -p "$INSTALL_DIR/wwwroot"
    # Sync files, excluding snapshots directory
    # Using cp with exclusion logic
    rsync -av --exclude='snapshots/' "$TMP_EXTRACT/wwwroot/" "$INSTALL_DIR/wwwroot/"
fi

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
