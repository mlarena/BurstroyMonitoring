import os
import re
import sys

def replace_in_files(root_dir, replacements):
    """Рекурсивно заменяет текст во всех файлах"""
    processed_files = 0
    changed_files = 0
    
    # Получаем полный путь к текущему скрипту
    current_script = os.path.abspath(sys.argv[0])
    
    for dirpath, dirnames, filenames in os.walk(root_dir):
        for filename in filenames:
            filepath = os.path.join(dirpath, filename)
            
            # Пропускаем текущий скрипт
            if os.path.abspath(filepath) == current_script:
                continue
            
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                original_content = content
                
                # Выполняем замены
                for old_word, new_word in replacements:
                    pattern = re.compile(re.escape(old_word))
                    content = pattern.sub(new_word, content)
                
                # Если содержимое изменилось, записываем файл
                if content != original_content:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    changed_files += 1
                    print(f"Изменен: {os.path.relpath(filepath, root_dir)}")
                
                processed_files += 1
                
            except (UnicodeDecodeError, IsADirectoryError, PermissionError):
                # Пропускаем бинарные файлы, директории и файлы без доступа
                continue
            except Exception as e:
                print(f"Ошибка обработки {filepath}: {e}")
    
    return processed_files, changed_files

def main():
    # Список замен
    replacements = [
        # ('Сенсоров', 'Датчиков'),
        # ('сенсоров', 'датчиков'),
        # ('сенсора', 'датчика'),        
        # ('сенсоры', 'датчики'),        
        # ('сенсоре', 'датчике'),        
        ('сенсор', 'датчик')
    ]
    
    current_dir = os.getcwd()
    
    print("Начинаю замену текста в файлах...")
    print(f"Директория: {current_dir}")
    print(f"Текущий скрипт исключен из обработки: {os.path.basename(sys.argv[0])}")
    print("Замены:")
    for old, new in replacements:
        print(f"  {old} → {new}")
    print()
    
    processed, changed = replace_in_files(current_dir, replacements)
    
    print(f"\nГотово!")
    print(f"Обработано файлов: {processed}")
    print(f"Изменено файлов: {changed}")

if __name__ == "__main__":
    main()