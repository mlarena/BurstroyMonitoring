// Глобальная переменная для отслеживания активного dropdown
let activeDropdown = null;

/**
 * Переключение видимости фильтра
 * @param {string} filterId - ID элемента фильтра
 */
function toggleFilter(filterId) {
    const dropdown = document.getElementById(filterId);
    
    // Если этот dropdown уже активен, просто закрываем его
    if (activeDropdown === dropdown && dropdown.classList.contains('show')) {
        closeAllDropdowns();
        return;
    }
    
    // Закрываем все другие dropdown
    closeAllDropdowns();
    
    // Открываем текущий dropdown
    dropdown.classList.add('show');
    activeDropdown = dropdown;
    
    // Блокируем скролл страницы
    document.body.style.overflow = 'hidden';
}

/**
 * Закрытие конкретного фильтра
 * @param {string} filterId - ID элемента фильтра
 */
function closeFilter(filterId) {
    const dropdown = document.getElementById(filterId);
    dropdown.classList.remove('show');
    
    if (activeDropdown === dropdown) {
        activeDropdown = null;
    }
    
    if (!document.querySelector('.filter-dropdown.show')) {
        document.body.style.overflow = '';
    }
}

/**
 * Закрытие всех открытых фильтров
 */
function closeAllDropdowns() {
    document.querySelectorAll('.filter-dropdown').forEach(dropdown => {
        dropdown.classList.remove('show');
    });
    
    activeDropdown = null;
    document.body.style.overflow = '';
}

/**
 * Применение фильтров и перенаправление на страницу с параметрами
 */
function applyFilter() {
    const form = document.querySelector('.search-form');
    if (!form) return;
    
    // Создаем новый URLSearchParams
    const params = new URLSearchParams();
    
    // 1. Добавляем search параметр
    const searchInput = form.querySelector('input[name="search"]');
    if (searchInput && searchInput.value) {
        params.set('search', searchInput.value);
    }
    
    // 2. Добавляем параметры пагинации и сортировки из скрытых полей
    const hiddenInputs = form.querySelectorAll('input[type="hidden"]');
    hiddenInputs.forEach(input => {
        // Пропускаем поля фильтров, они будут обработаны отдельно
        if (input.name !== 'selectedSensorTypes' && 
            input.name !== 'selectedMonitoringPosts' && 
            input.name !== 'sensorTypeId' && 
            input.name !== 'monitoringPostId') {
            if (input.name && input.value) {
                params.set(input.name, input.value);
            }
        }
    });
    
    // 3. Собираем выбранные типы датчиков (только те, что отмечены)
    const sensorTypeCheckboxes = document.querySelectorAll('input[name="selectedSensorTypes"]:checked');
    if (sensorTypeCheckboxes.length > 0) {
        // Используем Set для уникальности
        const uniqueSensorTypes = [...new Set(Array.from(sensorTypeCheckboxes).map(cb => cb.value))];
        uniqueSensorTypes.forEach(value => {
            params.append('selectedSensorTypes', value);
        });
    }
    
    // 4. Собираем выбранные посты мониторинга (только те, что отмечены)
    const monitoringPostCheckboxes = document.querySelectorAll('input[name="selectedMonitoringPosts"]:checked');
    if (monitoringPostCheckboxes.length > 0) {
        // Используем Set для уникальности
        const uniqueMonitoringPosts = [...new Set(Array.from(monitoringPostCheckboxes).map(cb => cb.value))];
        uniqueMonitoringPosts.forEach(value => {
            params.append('selectedMonitoringPosts', value);
        });
    }
    
    // 5. Проверяем, нужно ли использовать единичные параметры (sensorTypeId/monitoringPostId)
    // Если выбран только один тип датчика, используем sensorTypeId вместо selectedSensorTypes
    const uniqueSensorValues = [...new Set(Array.from(sensorTypeCheckboxes).map(cb => cb.value))];
    if (uniqueSensorValues.length === 1) {
        params.delete('selectedSensorTypes');
        params.set('sensorTypeId', uniqueSensorValues[0]);
    } else if (uniqueSensorValues.length > 1) {
        params.delete('sensorTypeId');
    }
    
    // Если выбран только один пост, используем monitoringPostId вместо selectedMonitoringPosts
    const uniquePostValues = [...new Set(Array.from(monitoringPostCheckboxes).map(cb => cb.value))];
    if (uniquePostValues.length === 1) {
        params.delete('selectedMonitoringPosts');
        params.set('monitoringPostId', uniquePostValues[0]);
    } else if (uniquePostValues.length > 1) {
        params.delete('monitoringPostId');
    }
    
    // 6. Убеждаемся, что page = 1 при изменении фильтров
    params.set('page', '1');
    
    // Формируем URL
    const queryString = params.toString();
    const currentAction = form.getAttribute('action') || window.location.pathname;
    const newUrl = queryString ? currentAction + '?' + queryString : currentAction;
    
    console.log('Navigating to:', newUrl); // Для отладки
    window.location.href = newUrl;
}

/**
 * Переключение фильтра по клику на элемент в таблице
 * @param {string} filterType - Тип фильтра ('sensorType' или 'monitoringPost')
 * @param {string} id - ID элемента
 */
function toggleFilterItem(filterType, id) {
    // Парсим текущие параметры URL
    const urlParams = new URLSearchParams(window.location.search);
    
    // Определяем параметры в зависимости от типа фильтра
    let singleParam, multipleParam;
    if (filterType === 'sensorType') {
        singleParam = 'sensorTypeId';
        multipleParam = 'selectedSensorTypes';
    } else {
        singleParam = 'monitoringPostId';
        multipleParam = 'selectedMonitoringPosts';
    }
    
    // Получаем текущие значения
    const currentSingleId = urlParams.get(singleParam);
    const currentMultipleIds = urlParams.getAll(multipleParam);
    
    // Создаем Set для уникальности
    const allCurrentIds = new Set();
    if (currentSingleId) allCurrentIds.add(currentSingleId);
    currentMultipleIds.forEach(id => allCurrentIds.add(id));
    
    // Проверяем, активен ли этот элемент
    const isActive = allCurrentIds.has(id);
    
    if (isActive) {
        // Если элемент активен - удаляем его
        allCurrentIds.delete(id);
    } else {
        // Если элемент не активен - добавляем его
        allCurrentIds.add(id);
    }
    
    // Очищаем все параметры этого типа
    urlParams.delete(singleParam);
    urlParams.delete(multipleParam);
    
    // Добавляем оставшиеся ID
    const remainingIds = Array.from(allCurrentIds);
    if (remainingIds.length === 1) {
        // Если остался только один ID, используем единичный параметр
        urlParams.set(singleParam, remainingIds[0]);
    } else if (remainingIds.length > 1) {
        // Если несколько ID, используем множественный параметр
        remainingIds.forEach(id => urlParams.append(multipleParam, id));
    }
    // Если remainingIds.length === 0, то ничего не добавляем - фильтр снят
    
    // Сбрасываем на первую страницу
    urlParams.set('page', '1');
    
    // Сохраняем остальные параметры (они уже в urlParams)
    
    // Формируем новый URL
    const queryString = urlParams.toString();
    const newUrl = queryString ? window.location.pathname + '?' + queryString : window.location.pathname;
    
    console.log('Toggle filter. New URL:', newUrl); // Для отладки
    window.location.href = newUrl;
}

/**
 * Сброс всех фильтров и перенаправление на базовый URL
 * @param {string} baseUrl - базовый URL для перенаправления
 */
function resetAllFilters(baseUrl) {
    window.location.href = baseUrl;
}

// Инициализация обработчиков событий после загрузки DOM
document.addEventListener('DOMContentLoaded', function() {
    // Закрытие dropdown при клике вне его области
    document.addEventListener('click', function(event) {
        if (activeDropdown && 
            !activeDropdown.contains(event.target) && 
            !event.target.closest('.filter-trigger-group .button')) {
            closeAllDropdowns();
        }
    });
    
    // Закрытие при нажатии Escape
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeAllDropdowns();
        }
    });
    
    // Предотвращаем закрытие при клике внутри dropdown
    document.querySelectorAll('.filter-dropdown').forEach(dropdown => {
        dropdown.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    });
    
    // Для отладки - показываем текущие параметры
    console.log('Current URL params:', window.location.search);
});