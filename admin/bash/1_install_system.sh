#!/bin/bash

echo "Starting installation and service setup..."

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

echo "Installation and service setup completed successfully!"