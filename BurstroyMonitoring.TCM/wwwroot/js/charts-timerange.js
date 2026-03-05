// Управление временными диапазонами с автоматической загрузкой данных
(function() {
    'use strict';
    
    console.log('Charts TimeRange JS loaded');
    
    // Функция для инициализации контролов временных диапазонов
    function initTimeRangeControls() {
        console.log('Initializing time range controls');
        
        // Находим все кнопки диапазонов
        const rangeBtns = document.querySelectorAll('.range-btn');
        
        if (rangeBtns.length === 0) {
            console.log('No range buttons found');
            return;
        }
        
        console.log(`Found ${rangeBtns.length} range buttons`);
        
        rangeBtns.forEach(btn => {
            // Удаляем старые обработчики
            btn.removeEventListener('click', handleRangeClick);
            // Добавляем новый обработчик
            btn.addEventListener('click', handleRangeClick);
            console.log(`Added click handler to button: ${btn.dataset.days} days for prefix: ${btn.dataset.prefix}`);
        });
    }
    
    // Обработчик клика по кнопке временного диапазона
    async function handleRangeClick(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const btn = e.currentTarget;
        console.log(`Range button clicked: ${btn.dataset.days} days`);
        
        // Защита от повторных кликов
        if (btn.disabled || btn.classList.contains('processing')) {
            console.log('Button is disabled or processing');
            return;
        }
        
        const days = parseInt(btn.dataset.days);
        const prefix = btn.dataset.prefix;
        
        if (!prefix || !days) {
            console.log('Missing prefix or days');
            return;
        }
        
        // Находим родительский контрол
        const control = btn.closest('.time-range-control');
        if (!control) {
            console.log('Parent control not found');
            return;
        }
        
        // Блокируем кнопку
        btn.disabled = true;
        btn.classList.add('processing');
        
        try {
            // Убираем active у всех кнопок в этой группе
            const buttons = control.querySelectorAll('.range-btn');
            buttons.forEach(b => {
                b.classList.remove('active');
                b.disabled = false;
                b.classList.remove('processing');
            });
            
            // Добавляем active текущей кнопке
            btn.classList.add('active');
            
            // Обновляем активный период в data-атрибуте
            control.dataset.activeDays = days;
            
            // Получаем ID сенсора из родительского контейнера
            const sensorCard = control.closest('.bsp-chart-sensor-card');
            if (!sensorCard) {
                console.log('Sensor card not found');
                return;
            }
            
            const sensorId = sensorCard.dataset.sensorId;
            const sensorType = sensorCard.dataset.sensorType;
            
            console.log(`Sensor ID: ${sensorId}, Type: ${sensorType}`);
            
            // Получаем текущий выбранный интервал агрегации
            const aggregationControl = document.querySelector(`.aggregation-interval-control[data-prefix="${prefix}"]`);
            let activeInterval = 'hour';
            
            if (aggregationControl) {
                activeInterval = aggregationControl.dataset.activeInterval || 'hour';
                console.log(`Found aggregation control with interval: ${activeInterval}`);
            } else {
                console.log(`Aggregation control not found for prefix: ${prefix}`);
            }
            
            // Обновляем отображение метода
            if (window.ChartsCommon) {
                console.log(`Updating method info: ${prefix}, ${sensorId}, ${activeInterval}, ${days}`);
                ChartsCommon.updateMethodInfo(prefix, sensorId, activeInterval, days);
                
                // Автоматически загружаем данные с новым периодом
                console.log(`Loading test data...`);
                await ChartsCommon.loadTestData(prefix, sensorId, activeInterval, days);
            } else {
                console.error('ChartsCommon not found');
            }
            
            // Диспатчим событие об изменении периода
            const event = new CustomEvent('timeRangeChanged', {
                detail: {
                    prefix: prefix,
                    days: days,
                    sensorId: sensorId,
                    interval: activeInterval
                }
            });
            document.dispatchEvent(event);
            
            console.log(`[TimeRange] ${prefix}: days changed to ${days}, auto-loaded data`);
            
        } catch (error) {
            console.error(`[TimeRange] Error: ${error.message}`);
        } finally {
            // Разблокируем кнопку
            btn.disabled = false;
            btn.classList.remove('processing');
        }
    }
    
    // Запускаем инициализацию после загрузки DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            console.log('DOM loaded, initializing time range controls');
            initTimeRangeControls();
        });
    } else {
        console.log('DOM already loaded, initializing time range controls now');
        initTimeRangeControls();
    }
    
    // Также инициализируем при динамической загрузке контента
    // Наблюдатель за изменениями в DOM
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.addedNodes.length) {
                // Проверяем, были ли добавлены новые кнопки диапазонов
                const hasNewRangeBtns = document.querySelectorAll('.range-btn').length > 0;
                if (hasNewRangeBtns) {
                    console.log('New range buttons detected, reinitializing');
                    initTimeRangeControls();
                }
            }
        });
    });
    
    // Начинаем наблюдение после загрузки страницы
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            observer.observe(document.body, { childList: true, subtree: true });
        });
    } else {
        observer.observe(document.body, { childList: true, subtree: true });
    }
    
    // Экспортируем функцию для возможного повторного вызова
    window.refreshTimeRangeControls = initTimeRangeControls;
})();