#!/bin/bash

echo "Starting system update..."

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

./start_services.sh
if [ $? -ne 0 ]; then
    echo "start_services.sh failed!"
    exit 1
fi

echo "System update completed successfully!"