#!/bin/bash

# Root check
if [ "$EUID" -ne 0 ]; then 
    echo "Please run as root: sudo $0"
    exit 1
fi

echo "=== Checking system dependencies for Burstroy RTSP System ==="

# Function to check and install a package
check_package() {
    PACKAGE=$1
    if command -v "$PACKAGE" >/dev/null 2>&1; then
        echo "✅ $PACKAGE is already installed."
    else
        echo "❌ $PACKAGE is NOT installed. Installing..."
        apt-get update
        apt-get install -y "$PACKAGE"
        if [ $? -eq 0 ]; then
            echo "✅ $PACKAGE installed successfully."
        else
            echo "❌ Failed to install $PACKAGE. Please check your internet connection or apt sources."
            exit 1
        fi
    fi
}

# Check FFmpeg (Critical for video processing)
check_package "ffmpeg"

# Check Unzip (Required for your unpack scripts)
check_package "unzip"

# Check Curl (Used in your setup scripts for health checks)
check_package "curl"

check_package "rsync"

check_package "sudo"

check_package "libicu-dev"

echo ""
echo "=== Dependency check complete ==="
echo "FFmpeg version: $(ffmpeg -version | head -n 1)"
echo ""
echo "System is ready for Burstroy RTSP services."
