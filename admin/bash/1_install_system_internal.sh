#!/bin/bash

echo "Starting installation and service setup..."

# Установка postgres
echo "=== check-dependencies.sh ==="
./check-dependencies.sh
if [ $? -ne 0 ]; then
    echo "check-dependencies.sh failed!"
    exit 1
fi

sudo timedatectl set-timezone Europe/Moscow

# Установите пакет с локалями (если не установлен)
sudo apt install locales

# Сгенерируйте русскую UTF-8 локаль
sudo locale-gen ru_RU.UTF-8

# Настройте локаль по умолчанию
sudo update-locale LANG=ru_RU.UTF-8 LC_ALL=ru_RU.UTF-8

# Установка setup_nginx
echo "=== setup_nginx.sh ==="
./setup_nginx.sh
if [ $? -ne 0 ]; then
    echo "setup_nginx.sh failed!"
    exit 1
fi

# Установка postgres
echo "=== postgres ==="
./setup_postgresql_internal.sh
if [ $? -ne 0 ]; then
    echo "setup_postgresql_internal.sh failed!"
    exit 1
fi

# Установка postgres
echo "=== setup_pg_timezone.sh ==="
./setup_pg_timezone.sh
if [ $? -ne 0 ]; then
    echo "setup_pg_timezone.sh failed!"
    exit 1
fi


# Установка компонентов
echo "=== Installing TCM ==="
./install_tcm.sh
if [ $? -ne 0 ]; then
    echo "install_tcm.sh failed!"
    exit 1
fi

echo "=== Installing Video Monitoring ==="
./install_video-monitoring.sh
if [ $? -ne 0 ]; then
    echo "install_video-monitoring.sh failed!"
    exit 1
fi

echo "=== Installing Worker ==="
./install_worker.sh
if [ $? -ne 0 ]; then
    echo "install_worker.sh failed!"
    exit 1
fi

echo "=== Installing API ==="
./install_api.sh
if [ $? -ne 0 ]; then
    echo "install_api.sh failed!"
    exit 1
fi

# Создание сервисов
echo "=== Creating TCM Service ==="
./create-service-burstroy-monitoring-tcm.sh
if [ $? -ne 0 ]; then
    echo "create-service-burstroy-monitoring-tcm.sh failed!"
    exit 1
fi

echo "=== Creating Video Monitoring Service ==="
./create-service-burstroy-monitoring-video.sh
if [ $? -ne 0 ]; then
    echo "create-service-burstroy-monitoring-video.sh failed!"
    exit 1
fi

echo "=== Creating Worker Service ==="
./create-service-burstroy-monitoring-worker.sh
if [ $? -ne 0 ]; then
    echo "create-service-burstroy-monitoring-worker.sh failed!"
    exit 1
fi

echo "=== Creating API Service ==="
./create-service-burstroy-monitoring-api.sh
if [ $? -ne 0 ]; then
    echo "create-service-burstroy-monitoring-api.sh failed!"
    exit 1
fi

# Установка setup_nginx
echo "=== setup-nginx-proxy.sh ==="
./setup-nginx-proxy.sh
if [ $? -ne 0 ]; then
    echo "setup-nginx-proxy.sh failed!"
    exit 1
fi


echo "Installation and service setup completed successfully!"