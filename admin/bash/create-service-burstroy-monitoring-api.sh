#!/bin/bash

# Settings for Api service
SERVICE_NAME="burstroy-monitoring-api"
APP_NAME="BurstroyMonitoring.Api"
INSTALL_DIR="/opt/burstroy/api"
USER_NAME="burstroy"
DESCRIPTION="Burstroy Monitoring API Service"
APP_PORT="5004"

# Parse command line arguments for port
while [[ $# -gt 0 ]]; do
    case $1 in
        --port|-p)
            APP_PORT="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--port <port_number>]"
            exit 1
            ;;
    esac
done

# Root check
if [ "$EUID" -ne 0 ]; then 
    echo "Please run as root: sudo $0 [--port <port>]"
    exit 1
fi

# Путь к исполняемому файлу
APP_EXEC="$INSTALL_DIR/$APP_NAME"

# Validate port
if ! [[ "$APP_PORT" =~ ^[0-9]+$ ]] || [ "$APP_PORT" -lt 1 ] || [ "$APP_PORT" -gt 65535 ]; then
    echo "Error: Invalid port number: $APP_PORT"
    exit 1
fi

# Check executable
if [ ! -f "$APP_EXEC" ]; then
    echo "Error: Application not found at $APP_EXEC"
    echo "Expected: $APP_NAME in $INSTALL_DIR/"
    exit 1
fi

echo "Setting up $SERVICE_NAME on port $APP_PORT..."

# Make the file executable
chmod +x "$APP_EXEC"

if ! getent group "$USER_NAME" &>/dev/null; then
    groupadd --system "$USER_NAME"
fi

if ! id "$USER_NAME" &>/dev/null; then
    useradd --system --no-create-home --shell /usr/sbin/nologin \
            --gid "$USER_NAME" "$USER_NAME"
    echo "Created user and group: $USER_NAME"
fi

# Set permissions
chown -R "$USER_NAME:$USER_NAME" "$INSTALL_DIR"
echo "Set permissions for $INSTALL_DIR to $USER_NAME"

# Create service with specified port
cat > /etc/systemd/system/$SERVICE_NAME.service << EOF
[Unit]
Description=$DESCRIPTION
After=network.target
Wants=network.target

[Service]
Type=simple  
User=$USER_NAME
Group=$USER_NAME
WorkingDirectory=$INSTALL_DIR
ExecStart=$APP_EXEC --urls http://*:$APP_PORT
Restart=always
RestartSec=10
TimeoutStartSec=60 
TimeoutStopSec=30
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
SyslogIdentifier=$SERVICE_NAME
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
EOF

echo "Created service file: /etc/systemd/system/$SERVICE_NAME.service"

systemctl daemon-reload
systemctl enable $SERVICE_NAME

echo "=== Starting service ==="
sudo systemctl restart $SERVICE_NAME
sleep 5

echo "=== Service Status ==="
sudo systemctl status $SERVICE_NAME --no-pager --lines=5
