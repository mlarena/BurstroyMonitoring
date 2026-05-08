#!/bin/bash

echo "Starting system update..."
sudo apt update

sudo apt install locales

# Сгенерируйте русскую UTF-8 локаль
sudo locale-gen ru_RU.UTF-8

# Настройте локаль по умолчанию
sudo update-locale LANG=ru_RU.UTF-8 LC_ALL=ru_RU.UTF-8

./update_api.sh
if [ $? -ne 0 ]; then
    echo "update_api.sh failed!"
    exit 1
fi

./update_tcm.sh
if [ $? -ne 0 ]; then
    echo "update_tcm.sh failed!"
    exit 1
fi

./update_video-monitoring.sh
if [ $? -ne 0 ]; then
    echo "update_video-monitoring.sh failed!"
    exit 1
fi

./update_worker.sh
if [ $? -ne 0 ]; then
    echo "update_worker.sh failed!"
    exit 1
fi

./3_start_services.sh
if [ $? -ne 0 ]; then
    echo "3_start_services.sh failed!"
    exit 1
fi

echo "System update completed successfully!"