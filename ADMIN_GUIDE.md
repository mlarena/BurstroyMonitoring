# Руководство администратора: BurstroyMonitoring

Данное руководство содержит инструкции по установке, настройке и обновлению всех компонентов системы `BurstroyMonitoring` на серверах под управлением Linux (Ubuntu/Debian).

## Состав системы
1.  **API**: Центральный интерфейс взаимодействия с базой данных.
2.  **TCM**: Модуль управления трафиком и конфигурациями.
3.  **Video Monitoring**: Сервис обработки видеопотоков.
4.  **Worker**: Фоновый сервис опроса датчиков метеостанций.

## Предварительные требования

Перед установкой убедитесь, что на сервере установлены:
*   **.NET 10.0 SDK/Runtime**
*   **PostgreSQL** (с созданной базой данных)
*   **Nginx** (для проксирования API)
*   **Git**

Для автоматической настройки окружения используйте скрипты из `admin/bash/`:
*   `setup_dotnet.sh` — установка .NET 10.
*   `setup_postgresql.sh` — установка и настройка СУБД.
*   `setup_nginx.sh` и `setup-nginx-proxy.sh` — настройка веб-сервера.

## Установка компонентов

Установка каждого модуля выполняется парой скриптов: `install_*.sh` (сборка и копирование файлов) и `create-service-*.sh` (создание службы systemd).

### 1. Установка API
```bash
cd admin/bash
chmod +x install_api.sh create-service-burstroy-monitoring-api.sh
./install_api.sh
./create-service-burstroy-monitoring-api.sh
```

### 2. Установка TCM
```bash
cd admin/bash
chmod +x install_tcm.sh create-service-burstroy-monitoring-tcm.sh
./install_tcm.sh
./create-service-burstroy-monitoring-tcm.sh
```

### 3. Установка Video Monitoring
```bash
cd admin/bash
chmod +x install_video-monitoring.sh create-service-burstroy-monitoring-video.sh
./install_video-monitoring.sh
./create-service-burstroy-monitoring-video.sh
```

### 4. Установка Worker
```bash
cd admin/bash
chmod +x install_worker.sh create-service-burstroy-monitoring-worker.sh
./install_worker.sh
./create-service-burstroy-monitoring-worker.sh
```

## Настройка (Configuration)

Файлы конфигурации `appsettings.json` находятся в соответствующих директориях установки (обычно `/var/www/burstroy-*`).

### Основные параметры:
*   **ConnectionStrings:DefaultConnection**: Строка подключения к БД (общая для всех модулей).
*   **Logging**: Настройки путей к логам.

После изменения любого `appsettings.json` перезапустите соответствующую службу:
```bash
sudo systemctl restart burstroy-monitoring-[api|tcm|video|worker]
```

## Обновление системы

Для обновления любого компонента до последней версии, используйте соответствующие скрипты обновления:

```bash
cd admin/bash
chmod +x update_*.sh
./update_api.sh
./update_tcm.sh
./update_video-monitoring.sh
./update_worker.sh
```

## Мониторинг и диагностика

### Проверка статуса служб:
```bash
sudo systemctl status "burstroy-monitoring-*"
```

### Просмотр логов:
```bash
# Для API
journalctl -u burstroy-monitoring-api -f
# Для Worker
journalctl -u burstroy-monitoring-worker -f
```

### Типовые проблемы:
1.  **Ошибка 502 (Nginx)**: Проверьте, запущен ли сервис API (`systemctl status burstroy-monitoring-api`).
2.  **Ошибка подключения к БД**: Проверьте `ConnectionStrings` во всех `appsettings.json`.
3.  **Права доступа**: Скрипты установки настраивают права на `/var/www/`, убедитесь, что они не были изменены вручную.
