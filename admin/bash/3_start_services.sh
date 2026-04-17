#!/bin/bash

services="tcm worker video api"

for s in $services; do
    sudo systemctl start "burstroy-monitoring-$s"
    echo "Started burstroy-monitoring-$s"
done

echo -e "\nAll services started. Checking status:"
systemctl status burstroy-monitoring-{tcm,worker,video,api} --no-pager | grep -E "●|Active:"