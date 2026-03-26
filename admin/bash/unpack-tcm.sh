#!/bin/bash

# Settings for TCM service
APP_NAME="BurstroyMonitoring.TCM"
ZIP_FILE="BurstroyMonitoring.TCM.zip"
INSTALL_DIR="/opt/burstroy/tcm"
USER_NAME="burstroy"

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

echo "Updating $APP_NAME in $INSTALL_DIR..."

# Create directory if not exists
mkdir -p "$INSTALL_DIR"

# Stop service before update if it exists
if systemctl is-active --quiet burstroy-tcm; then
    echo "Stopping burstroy-tcm service..."
    systemctl stop burstroy-tcm
fi

# Clear directory (except appsettings.json to preserve config)
echo "Cleaning $INSTALL_DIR..."
find "$INSTALL_DIR" -mindepth 1 ! -name 'appsettings.json' -delete

# Unzip
echo "Unpacking $ZIP_FILE..."
unzip -o "$ZIP_FILE" -d "$INSTALL_DIR"

# Make executable
echo "Setting permissions..."
chmod +x "$INSTALL_DIR/$APP_NAME"
chown -R "$USER_NAME:$USER_NAME" "$INSTALL_DIR"

echo "✅ $APP_NAME updated successfully."
echo "You can now start the service: sudo systemctl start burstroy-tcm"
