#!/bin/bash
mkdir -p /etc/postgresql/18/main/conf.d
cat > /etc/postgresql/18/main/conf.d/99-timezone.conf << EOF
timezone = 'Europe/Moscow'
log_timezone = 'Europe/Moscow'
EOF
chown postgres:postgres /etc/postgresql/18/main/conf.d/99-timezone.conf
systemctl restart postgresql