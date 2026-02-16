#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Скрипт для генерации SQL запросов INSERT из файла с табуляцией
Формат: ID[TAB]Город[TAB]Широта[TAB]Долгота
"""

import re
from datetime import datetime

def generate_sql_from_tsv(input_file, output_file='insert_queries.sql', start_id=1):
    """
    Генерирует SQL запросы из файла с табуляцией
    """
    
    cities = []
    
    # Чтение файла
    with open(input_file, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    for line in lines:
        # Пропускаем пустые строки
        line = line.strip()
        if not line:
            continue
            
        # Разделяем по табуляции или нескольким пробелам
        if '\t' in line:
            parts = line.split('\t')
        else:
            # Если нет табуляции, разделяем по 2+ пробелам
            parts = re.split(r'\s{2,}', line)
        
        if len(parts) >= 4:
            try:
                city_id = int(parts[0].strip())
                city_name = parts[1].strip()
                latitude = float(parts[2].strip())
                longitude = float(parts[3].strip())
                
                cities.append({
                    'id': city_id,
                    'name': city_name,
                    'lat': latitude,
                    'lon': longitude
                })
            except (ValueError, IndexError) as e:
                print(f"Ошибка обработки строки: {line}")
                print(f"Ошибка: {e}")
                continue
    
    if not cities:
        print("Не удалось загрузить данные из файла")
        return
    
    # Генерация SQL
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write("-- SQL запросы для вставки данных мониторинговых постов\n")
        f.write("-- Сгенерировано: " + datetime.now().strftime("%Y-%m-%d %H:%M:%S") + "\n")
        f.write("-- Количество постов: " + str(len(cities)) + "\n\n")
        
        # Определяем, какие посты будут мобильными (каждый 5-й)
        mobile_indices = {i for i in range(4, len(cities), 5)}
        
        for i, city in enumerate(cities, start=start_id):
            is_mobile = (i - start_id) in mobile_indices
            mobile_status = "мобильный" if is_mobile else "стационарный"
            
            # Формируем название и описание
            post_name = f"{city['name']}"
            description = f"{mobile_status.capitalize()} пост мониторинга в г. {city['name']} (ID: {i:03d})"
            
            # Экранируем апострофы в названиях
            post_name = post_name.replace("'", "''")
            description = description.replace("'", "''")
            
            # SQL запрос
            sql = f"""INSERT INTO public."MonitoringPost" 
    ("Name", "Description", "Longitude", "Latitude", "IsMobile", "IsActive", "CreatedAt", "UpdatedAt")
VALUES
    ('{post_name}', '{description}', {city['lon']}, {city['lat']}, {str(is_mobile).lower()}, true, NOW(), NOW());\n"""
            
            f.write(sql)
            f.write("\n")
    
    print(f"✓ Успешно обработано городов: {len(cities)}")
    print(f"✓ Сгенерировано SQL запросов: {len(cities)}")
    print(f"✓ Мобильных постов: {len(mobile_indices)}")
    print(f"✓ Стационарных постов: {len(cities) - len(mobile_indices)}")
    print(f"✓ Результат сохранен в: {output_file}")
    
    # Показать пример
    print("\nПример сгенерированного запроса:")
    with open(output_file, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        for j in range(min(10, len(lines))):
            if lines[j].strip() and not lines[j].startswith('--'):
                print(lines[j], end='')
                break

# Автоматический запуск для вашего файла
if __name__ == "__main__":
    import sys
    
    # Проверяем аргументы командной строки
    if len(sys.argv) > 1:
        input_filename = sys.argv[1]
    else:
        input_filename = input("Введите имя файла с данными о городах: ").strip()
        if not input_filename:
            # Создаем тестовый файл с вашими данными
            input_filename = "cities_data.tsv"
            test_data = """1	Москва	55.755826	37.6173
2	Санкт-Петербург	59.9342802	30.3350986
3	Новосибирск	55.030199	82.92043
4	Екатеринбург	56.8389261	60.6057025
5	Казань	55.7943877	49.1115312
6	Нижний Новгород	56.326887	44.005986
7	Челябинск	55.1644419	61.4368432
8	Красноярск	56.009097	92.8725192
9	Самара	53.2415041	50.2212463
10	Уфа	54.735147	55.958727
11	Ростов-на-Дону	47.2224445	39.7187862"""
            
            with open(input_filename, 'w', encoding='utf-8') as f:
                f.write(test_data)
            print(f"Создан тестовый файл: {input_filename}")
    
    output_filename = input("Введите имя выходного SQL файла (по умолчанию: monitoring_posts.sql): ").strip()
    if not output_filename:
        output_filename = "monitoring_posts.sql"
    
    try:
        generate_sql_from_tsv(input_filename, output_filename)
        print("\n✅ Готово!")
    except FileNotFoundError:
        print(f"❌ Ошибка: Файл '{input_filename}' не найден.")
        print("\nСоздайте файл с данными в формате:")
        print("ID[TAB]Город[TAB]Широта[TAB]Долгота")
        print("Пример:")
        print("1\tМосква\t55.755826\t37.6173")
        print("2\tСанкт-Петербург\t59.9342802\t30.3350986")
    except Exception as e:
        print(f"❌ Произошла ошибка: {e}")