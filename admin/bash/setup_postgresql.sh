#!/bin/bash

echo "Установка PostgreSQL 18..."

# Установка необходимых пакетов
apt update
apt install -y curl gnupg2

# Добавление репозитория PostgreSQL 18
echo "Добавление репозитория PostgreSQL 18..."
sh -c 'echo "deb [signed-by=/usr/share/keyrings/postgresql-keyring.gpg] http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'

# Импорт GPG ключа
curl -fsSL https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --dearmor -o /usr/share/keyrings/postgresql-keyring.gpg

# Обновление списка пакетов
apt update

# Установка PostgreSQL 18
apt install -y postgresql-18 postgresql-client-18 postgresql-contrib-18

# Запуск и включение службы PostgreSQL
systemctl start postgresql@18-main
systemctl enable postgresql@18-main

# Изменение пароля пользователя postgres
sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD '12345678';"

# Создание базы данных
sudo -u postgres psql -c "create database sensordb;"

sudo -u postgres psql -d sensordb -f /tmp/create_database.sql
sudo -u postgres psql -d sensordb -f /tmp/db_insert_internal_test_MonitoringPost.sql
sudo -u postgres psql -d sensordb -f /tmp/db_insert_sensors_default.sql
#sudo -u postgres psql -d sensordb -f /tmp/db_insert_test_sensors.sql


echo ""
echo "=== Установка завершена ==="
echo "Пользователь: postgres"
echo "Пароль: 12345678"
echo ""

# Вывод версии
echo "Версия PostgreSQL:"
sudo -u postgres psql -c "SELECT version();" | head -n 5
