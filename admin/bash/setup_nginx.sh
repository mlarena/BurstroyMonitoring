sudo apt update
sudo apt install nginx -y

# Настройка конфигурации по умолчанию (слушает все запросы)
echo "server {
    listen 80 default_server;
    listen [::]:80 default_server;

    root /var/www/html;
    index index.html index.htm;

    server_name _;

    location / {
        try_files \$uri \$uri/ =404;
    }
}" | sudo tee /etc/nginx/sites-available/default

sudo systemctl enable nginx
sudo systemctl start nginx
sudo systemctl reload nginx

nginx -v
sudo nginx -t