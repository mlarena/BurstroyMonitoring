#!/bin/bash
# Скрипт распаковки BurstroyMonitoring
# Запускается от root
apt install zip unzip
set -e  # Прерывать выполнение при ошибках

# Пути
TMP_DIR="/opt/burstroy/tmp"
TCM_DIR="/opt/burstroy/tcm"
WORKER_DIR="/opt/burstroy/worker"
TCM_ZIP="BurstroyMonitoring.TCM.zip"
WORKER_ZIP="BurstroyMonitoring.Worker.zip"
TCM_BIN="BurstroyMonitoring.TCM"
WORKER_BIN="BurstroyMonitoring.Worker"

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Функция для вывода сообщений
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Проверка, что скрипт запущен от root
check_root() {
    if [[ $EUID -ne 0 ]]; then
        log_error "Этот скрипт должен быть запущен от root!"
        exit 1
    fi
}

# Проверка существования исходных ZIP файлов
check_source_files() {
    if [[ ! -f "$TMP_DIR/$TCM_ZIP" ]]; then
        log_error "Файл $TCM_ZIP не найден в $TMP_DIR"
        exit 1
    fi
    
    if [[ ! -f "$TMP_DIR/$WORKER_ZIP" ]]; then
        log_error "Файл $WORKER_ZIP не найден в $TMP_DIR"
        exit 1
    fi
    
    log_info "Исходные ZIP файлы найдены"
}

# Создание целевых директорий, если они не существуют
create_directories() {
    for dir in "$TCM_DIR" "$WORKER_DIR"; do
        if [[ ! -d "$dir" ]]; then
            log_info "Создание директории: $dir"
            mkdir -p "$dir"
            chmod 755 "$dir"
        else
            log_info "Директория уже существует: $dir"
        fi
    done
}

# Распаковка файлов
extract_files() {
    log_info "Распаковка $TCM_ZIP в $TCM_DIR..."
    
    # Распаковка TCM
    if unzip -o "$TMP_DIR/$TCM_ZIP" -d "$TCM_DIR" > /dev/null 2>&1; then
        log_info "$TCM_ZIP успешно распакован"
    else
        log_error "Ошибка при распаковке $TCM_ZIP"
        exit 1
    fi
    
    log_info "Распаковка $WORKER_ZIP в $WORKER_DIR..."
    
    # Распаковка Worker
    if unzip -o "$TMP_DIR/$WORKER_ZIP" -d "$WORKER_DIR" > /dev/null 2>&1; then
        log_info "$WORKER_ZIP успешно распакован"
    else
        log_error "Ошибка при распаковке $WORKER_ZIP"
        exit 1
    fi
}

# Установка прав исполнения
set_executable_permissions() {
    log_info "Установка прав исполнения..."
    
    # Для TCM
    if [[ -f "$TCM_DIR/$TCM_BIN" ]]; then
        chmod +x "$TCM_DIR/$TCM_BIN"
        log_info "Права исполнения установлены для $TCM_BIN"
    else
        log_warn "Файл $TCM_BIN не найден в $TCM_DIR"
        # Поиск возможного исполняемого файла
        find "$TCM_DIR" -type f -executable -name "Burstroy*" | while read -r file; do
            log_info "Найден исполняемый файл: $(basename "$file")"
        done
    fi
    
    # Для Worker
    if [[ -f "$WORKER_DIR/$WORKER_BIN" ]]; then
        chmod +x "$WORKER_DIR/$WORKER_BIN"
        log_info "Права исполнения установлены для $WORKER_BIN"
    else
        log_warn "Файл $WORKER_BIN не найден в $WORKER_DIR"
        # Поиск возможного исполняемого файла
        find "$WORKER_DIR" -type f -executable -name "Burstroy*" | while read -r file; do
            log_info "Найден исполняемый файл: $(basename "$file")"
        done
    fi
}

# Установка владельца и прав доступа
set_ownership() {
    log_info "Установка владельца debusr:debusr..."
    
    # Определение пользователя debusr
    if id "debusr" &>/dev/null; then
        # Рекурсивная установка владельца
        chown -R debusr:debusr "$TCM_DIR"
        chown -R debusr:debusr "$WORKER_DIR"
        log_info "Владелец установлен успешно"
    else
        log_warn "Пользователь debusr не существует, оставляю root как владельца"
    fi
}

# Проверка результата
verify_extraction() {
    log_info "\n=== ПРОВЕРКА РЕЗУЛЬТАТА ==="
    
    echo "Содержимое $TCM_DIR:"
    ls -la "$TCM_DIR" 2>/dev/null || echo "Директория не существует"
    
    echo -e "\nСодержимое $WORKER_DIR:"
    ls -la "$WORKER_DIR" 2>/dev/null || echo "Директория не существует"
    
    echo -e "\nПроверка прав исполнения:"
    for dir in "$TCM_DIR" "$WORKER_DIR"; do
        if [[ -d "$dir" ]]; then
            echo "Исполняемые файлы в $dir:"
            find "$dir" -type f -executable -name "Burstroy*" -exec ls -la {} \; 2>/dev/null || echo "Нет исполняемых файлов"
        fi
    done
}

# Основная функция
main() {
    echo "========================================"
    echo "   СКРИПТ РАСПАКОВКИ BURSTROY MONITORING"
    echo "========================================"
    
    check_root
    check_source_files
    create_directories
    extract_files
    set_executable_permissions
    set_ownership
    verify_extraction
    
    log_info "\nРаспаковка завершена успешно!"
    echo "========================================"
}

# Запуск основной функции
main "$@"


