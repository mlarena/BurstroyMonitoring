#!/bin/bash

# Configuration
SERVICE_NAME="burstroy-monitoring-ui"
APP_PORT="5002"
NGINX_CONFIG_NAME="burstroy-ui"
DOMAIN_NAME="_"  # Use '_' for all domains, or specify actual domain

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function for colored output
print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

# Root check
if [ "$EUID" -ne 0 ]; then 
    print_error "Please run as root: sudo $0"
    exit 1
fi

echo "========================================"
echo "  Nginx Reverse Proxy Setup for Burstroy UI"
echo "========================================"
echo ""

# Step 1: Check if the service is running
print_status "Step 1: Checking if $SERVICE_NAME is running..."
if ! systemctl is-active --quiet $SERVICE_NAME; then
    print_warning "Service $SERVICE_NAME is not running"
    read -p "Do you want to start it now? (y/n): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        sudo systemctl start $SERVICE_NAME
        sleep 2
        if ! systemctl is-active --quiet $SERVICE_NAME; then
            print_error "Failed to start $SERVICE_NAME"
            exit 1
        fi
        print_status "Service started successfully"
    else
        print_error "Cannot proceed without running service"
        exit 1
    fi
else
    print_status "Service $SERVICE_NAME is running"
fi

# Step 2: Verify application is accessible
print_status "Step 2: Verifying application on port $APP_PORT..."
if ! curl -s -o /dev/null -w "%{http_code}" http://localhost:$APP_PORT | grep -q "200\|302\|301"; then
    print_error "Application is not responding on port $APP_PORT"
    print_warning "Please check: sudo journalctl -u $SERVICE_NAME"
    exit 1
fi
print_status "Application is responding on port $APP_PORT"

# Step 3: Create nginx configuration
print_status "Step 3: Creating nginx configuration..."

# Check for custom domain
read -p "Enter domain name (press Enter for all domains): " user_domain
if [ -n "$user_domain" ]; then
    DOMAIN_NAME="$user_domain"
    print_status "Using domain: $DOMAIN_NAME"
else
    print_status "Using all domains (_)"
fi

# Create nginx config
cat > /tmp/$NGINX_CONFIG_NAME.conf << EOF
server {
    listen 80;
    server_name $DOMAIN_NAME;
    
    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_proxied expired no-cache no-store private auth;
    gzip_types text/plain text/css text/xml text/javascript application/x-javascript application/xml application/javascript application/json;
    gzip_disable "MSIE [1-6]\.";
    
    location / {
        proxy_pass http://localhost:$APP_PORT;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header X-Forwarded-Host \$host;
        proxy_set_header X-Forwarded-Port \$server_port;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
        
        # Buffers
        client_max_body_size 100M;
        proxy_buffer_size 128k;
        proxy_buffers 4 256k;
        proxy_busy_buffers_size 256k;
    }
    
    # Static files optimization (if wwwroot exists)
    location ~* \.(css|js|jpg|jpeg|png|gif|ico|svg|woff|woff2|ttf|eot)$ {
        root /opt/burstroy/ui/wwwroot;
        expires 30d;
        add_header Cache-Control "public, immutable";
        try_files \$uri \$uri/ @proxy;
    }
    
    location @proxy {
        proxy_pass http://localhost:$APP_PORT;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
    }
    
    # Block hidden files
    location ~ /\. {
        deny all;
        access_log off;
        log_not_found off;
    }
    
    # Health check endpoint
    location /health {
        proxy_pass http://localhost:$APP_PORT/health;
        proxy_set_header Host \$host;
        access_log off;
    }
}
EOF

# Move config to nginx
sudo mv /tmp/$NGINX_CONFIG_NAME.conf /etc/nginx/sites-available/$NGINX_CONFIG_NAME
print_status "Configuration created: /etc/nginx/sites-available/$NGINX_CONFIG_NAME"

# Step 4: Enable site
print_status "Step 4: Enabling nginx site..."
if [ -L /etc/nginx/sites-enabled/default ]; then
    sudo rm /etc/nginx/sites-enabled/default
    print_status "Removed default nginx site"
fi

sudo ln -sf /etc/nginx/sites-available/$NGINX_CONFIG_NAME /etc/nginx/sites-enabled/
print_status "Site enabled"

# Step 5: Test and restart nginx
print_status "Step 5: Testing nginx configuration..."
if ! sudo nginx -t; then
    print_error "Nginx configuration test failed"
    print_warning "Check configuration: sudo nano /etc/nginx/sites-available/$NGINX_CONFIG_NAME"
    exit 1
fi

print_status "Restarting nginx..."
sudo systemctl restart nginx

if ! systemctl is-active --quiet nginx; then
    print_error "Nginx failed to restart"
    print_warning "Check logs: sudo journalctl -u nginx"
    exit 1
fi

print_status "Nginx restarted successfully"

# Step 6: Configure firewall (optional)
print_status "Step 6: Configuring firewall..."
if command -v ufw &> /dev/null && ufw status | grep -q "active"; then
    if ! ufw status | grep -q "80.*ALLOW"; then
        sudo ufw allow 80/tcp
        print_status "Firewall rule added for port 80"
    else
        print_status "Port 80 is already allowed in firewall"
    fi
else
    print_warning "UFW not active or not installed, skipping firewall configuration"
fi

# Step 7: Verify setup
print_status "Step 7: Verifying setup..."
sleep 2

echo ""
echo "Testing HTTP connection..."
HTTP_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost)
if [[ "$HTTP_RESPONSE" =~ ^(200|301|302)$ ]]; then
    print_status "✅ SUCCESS: Application is accessible on port 80"
    print_status "HTTP Response Code: $HTTP_RESPONSE"
else
    print_warning "⚠️  Warning: Got HTTP $HTTP_RESPONSE, expected 200, 301, or 302"
    print_warning "Application might be accessible but returned unexpected status"
fi

# Step 8: Show summary
echo ""
echo "========================================"
print_status "SETUP COMPLETED SUCCESSFULLY"
echo "========================================"
echo ""
echo "Summary:"
echo "  • Application: $SERVICE_NAME"
echo "  • Backend port: $APP_PORT"
echo "  • Frontend port: 80"
echo "  • Nginx config: /etc/nginx/sites-available/$NGINX_CONFIG_NAME"
echo "  • Domain/Server: $DOMAIN_NAME"
echo ""
echo "Management commands:"
echo "  • View nginx logs:    sudo tail -f /var/log/nginx/access.log"
echo "  • View error logs:    sudo tail -f /var/log/nginx/error.log"
echo "  • Restart nginx:      sudo systemctl restart nginx"
echo "  • Check nginx status: sudo systemctl status nginx"
echo "  • Check app status:   sudo systemctl status $SERVICE_NAME"
echo ""
echo "Test commands:"
echo "  • Test locally:       curl -I http://localhost"
echo "  • Test from server:   curl -I http://$(hostname -I | awk '{print $1}')"
echo ""
echo "Note: If using domain name, update DNS to point to this server's IP address"