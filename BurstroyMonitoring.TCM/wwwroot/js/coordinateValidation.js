// coordinateValidation.js
// Универсальная валидация координат для всех форм

console.log('Загрузка скрипта валидации координат...');

/**
 * Инициализация валидации координат для формы
 * @param {string} formId - ID формы
 */
function initCoordinateValidation(formId) {
    console.log(`Инициализация валидации координат для формы: ${formId}`);
    
    const form = document.getElementById(formId);
    if (!form) {
        console.error(`Форма с ID "${formId}" не найдена`);
        return null;
    }

    const longitudeInput = form.querySelector('input[data-field="longitude"]');
    const latitudeInput = form.querySelector('input[data-field="latitude"]');
    const coordRowExp = document.getElementById('coordRowExp');
    
    if (!longitudeInput || !latitudeInput) {
        console.error(`Не найдены поля координат. Longitude: ${!!longitudeInput}, Latitude: ${!!latitudeInput}`);
        return null;
    }
    
    console.log('Найдены поля координат:', {
        longitude: longitudeInput.name,
        latitude: latitudeInput.name,
        longitudeId: longitudeInput.id,
        latitudeId: latitudeInput.id
    });
    
    let warningDiv = null;

    // Заменяем запятую на точку при вводе
    function handleCoordinateInput(e) {
        const input = e.target;
        const cursorPosition = input.selectionStart;
        const oldValue = input.value;
        
        // Заменяем запятую на точку
        const newValue = oldValue.replace(/,/g, '.');
        
        if (newValue !== oldValue) {
            input.value = newValue;
            input.selectionStart = cursorPosition;
            input.selectionEnd = cursorPosition;
        }
        
        checkCoordinatePair();
    }

    // Проверка парности координат
    function checkCoordinatePair() {
        const lonValue = longitudeInput.value.trim();
        const latValue = latitudeInput.value.trim();
        
        // Удаляем старое предупреждение
        if (warningDiv && warningDiv.parentNode) {
            warningDiv.parentNode.removeChild(warningDiv);
            warningDiv = null;
        }
        
        // Если оба пустые или оба заполненные - ок
        if ((!lonValue && !latValue) || (lonValue && latValue)) {
            return true;
        }
        
        // Если одно заполнено, а другое пустое - показываем предупреждение
        const message = 'Заполните обе координаты или оставьте оба поля пустыми';
        
        warningDiv = document.createElement('div');
        warningDiv.className = 'alert-warning';
        warningDiv.innerHTML = `<strong>Внимание!</strong> ${message}`;
        warningDiv.style.cssText = 'margin-top: 10px; padding: 10px; border-radius: 4px; background-color: #fff8e5; border-left: 4px solid #ff8b00; color: #5e6c84; width: 100%; box-sizing: border-box;';
        
        // Добавляем в координатный контейнер
        if (coordRowExp) {
            coordRowExp.innerHTML = '';
            coordRowExp.appendChild(warningDiv);
        }
        
        return false;
    }

    // Валидация при потере фокуса
    function handleCoordinateBlur(e) {
        const input = e.target;
        const valueStr = input.value.trim();
        const fieldName = input.getAttribute('data-field');
        const fieldDisplayName = fieldName === 'longitude' ? 'Долгота' : 'Широта';
        const min = fieldName === 'longitude' ? -180 : -90;
        const max = fieldName === 'longitude' ? 180 : 90;
        
        const errorSpan = input.nextElementSibling;
        
        // Если поле пустое - очищаем ошибку
        if (valueStr === '') {
            if (errorSpan && errorSpan.classList.contains('field-validation-error')) {
                errorSpan.textContent = '';
                errorSpan.style.display = 'none';
                input.classList.remove('input-error');
            }
            return;
        }
        
        // Пытаемся преобразовать значение
        const numericValue = parseFloat(valueStr.replace(/,/g, '.'));
        
        // Проверяем число
        if (isNaN(numericValue)) {
            if (errorSpan && errorSpan.classList.contains('field-validation-error')) {
                errorSpan.textContent = `${fieldDisplayName} должно быть числом`;
                errorSpan.style.display = 'block';
                input.classList.add('input-error');
            }
            return;
        }
        
        // Проверяем диапазон
        if (numericValue < min || numericValue > max) {
            if (errorSpan && errorSpan.classList.contains('field-validation-error')) {
                errorSpan.textContent = `${fieldDisplayName} должно быть от ${min} до ${max}`;
                errorSpan.style.display = 'block';
                input.classList.add('input-error');
            }
            return;
        }
        
        // Если все ок - очищаем ошибку
        if (errorSpan && errorSpan.classList.contains('field-validation-error')) {
            errorSpan.textContent = '';
            errorSpan.style.display = 'none';
            input.classList.remove('input-error');
        }
        
        checkCoordinatePair();
    }

    // Проверка валидности полей (без блокировки отправки)
    function validateCoordinates() {
        let isValid = true;
        
        // Проверяем парность
        if (!checkCoordinatePair()) {
            isValid = false;
        }
        
        // Проверяем каждое заполненное поле
        [longitudeInput, latitudeInput].forEach(input => {
            if (input.value.trim() !== '') {
                const valueStr = input.value.trim();
                const fieldName = input.getAttribute('data-field');
                const min = fieldName === 'longitude' ? -180 : -90;
                const max = fieldName === 'longitude' ? 180 : 90;
                
                const numericValue = parseFloat(valueStr.replace(/,/g, '.'));
                
                if (isNaN(numericValue) || numericValue < min || numericValue > max) {
                    isValid = false;
                }
            }
        });
        
        return isValid;
    }

    // Обработка отправки формы - только предупреждение, не блокировка
    function handleFormSubmit(e) {
        console.log('Обработка отправки формы...');
        
        // Нормализуем запятые
        [longitudeInput, latitudeInput].forEach(input => {
            if (input.value.includes(',')) {
                input.value = input.value.replace(/,/g, '.');
            }
        });
        
        // Проверяем валидность
        const isValid = validateCoordinates();
        
        if (!isValid) {
            console.log('Координаты невалидны, но форма будет отправлена для серверной валидации');
            // НЕ блокируем отправку - пусть серверная валидация работает
            // e.preventDefault();
            // return false;
            
            // Просто показываем сообщение
            alert('Проверьте правильность введенных координат. Форма будет отправлена, но могут возникнуть ошибки.');
        }
        
        return true;
    }

    // Назначаем обработчики
    longitudeInput.addEventListener('input', handleCoordinateInput);
    latitudeInput.addEventListener('input', handleCoordinateInput);
    longitudeInput.addEventListener('blur', handleCoordinateBlur);
    latitudeInput.addEventListener('blur', handleCoordinateBlur);
    
    // Убираем старый обработчик если был
    form.removeEventListener('submit', handleFormSubmit);
    // Добавляем новый обработчик (только для нормализации запятых)
    form.addEventListener('submit', function(e) {
        // Нормализуем запятые
        [longitudeInput, latitudeInput].forEach(input => {
            if (input.value.includes(',')) {
                input.value = input.value.replace(/,/g, '.');
            }
        });
        // НЕ блокируем отправку
        return true;
    });
    
    // Проверяем парность при загрузке
    setTimeout(checkCoordinatePair, 100);
    
    console.log('Валидация координат инициализирована успешно');
    
    return {
        validate: validateCoordinates,
        checkPair: checkCoordinatePair
    };
}

// Автоматическая инициализация для всех форм с координатами
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM загружен, инициализация валидации координат...');
    
    // Находим все формы с полями координат
    const formsWithCoords = Array.from(document.querySelectorAll('form')).filter(form => {
        return form.querySelector('input[data-field="longitude"]') && 
               form.querySelector('input[data-field="latitude"]');
    });
    
    console.log(`Найдено форм с координатами: ${formsWithCoords.length}`);
    
    formsWithCoords.forEach((form, index) => {
        const formId = form.id;
        if (formId) {
            console.log(`${index + 1}. Инициализация валидации для формы: ${formId}`);
            initCoordinateValidation(formId);
        } else {
            console.log(`${index + 1}. Форма без ID, пропускаем`);
        }
    });
    
    if (formsWithCoords.length === 0) {
        console.log('Формы с координатами не найдены');
    }
});