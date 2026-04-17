#!/bin/bash

# Цвета для вывода
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

# Массив сервисов
services=("tcm" "worker" "video" "api")

for service in "${services[@]}"; do
    full_name="burstroy-monitoring-${service}"
    echo -e "${YELLOW}=== $full_name ===${NC}"
    
    # Получаем статус
    status=$(systemctl is-active "$full_name" 2>/dev/null)
    loaded=$(systemctl show "$full_name" -p LoadState --value 2>/dev/null)
    
    # Проверяем статус
    if [[ "$loaded" == "loaded" ]]; then
        echo -e "Loaded: ${GREEN}loaded${NC}"
    else
        echo -e "Loaded: ${RED}$loaded${NC}"
    fi
    
    if [[ "$status" == "active" ]]; then
        echo -e "Active: ${GREEN}active (running)${NC}"
    elif [[ "$status" == "inactive" ]]; then
        echo -e "Active: ${RED}inactive (dead)${NC}"
    elif [[ "$status" == "failed" ]]; then
        echo -e "Active: ${RED}failed${NC}"
    else
        echo -e "Active: ${YELLOW}$status${NC}"
    fi
    
    echo ""
done