// Управление интервалами агрегации с автоматической загрузкой данных
(function() {
    'use strict';
    
    console.log('Charts Aggregation JS loaded');
    
    // Функция для инициализации контролов агрегации
    function initAggregationControls() {
        console.log('Initializing aggregation controls');
        
        // Находим все кнопки агрегации
        const aggregationBtns = document.querySelectorAll('.aggregation-btn');
        
        if (aggregationBtns.length === 0) {
            console.log('No aggregation buttons found');
            return;
        }
        
        console.log(`Found ${aggregationBtns.length} aggregation buttons`);
        
        aggregationBtns.forEach(btn => {
            // Удаляем старые обработчики
            btn.removeEventListener('click', handleAggregationClick);
            // Добавляем новый обработчик
            btn.addEventListener('click', handleAggregationClick);
            console.log(`Added click handler to button: ${btn.dataset.interval} for prefix: ${btn.dataset.prefix}`);
        });
    }
    
    // Обработчик клика по кнопке агрегации
    async function handleAggregationClick(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const btn = e.currentTarget;
        console.log(`Aggregation button clicked: ${btn.dataset.interval}`);
        
        // Защита от повторных кликов
        if (btn.disabled || btn.classList.contains('processing')) {
            console.log('Button is disabled or processing');
            return;
        }
        
        const interval = btn.dataset.interval;
        const prefix = btn.dataset.prefix;
        
        if (!prefix || !interval) {
            console.log('Missing prefix or interval');
            return;
        }
        
        // Находим родительский контрол
        const control = btn.closest('.aggregation-interval-control');
        if (!control) {
            console.log('Parent control not found');
            return;
        }
        
        // Блокируем кнопку
        btn.disabled = true;
        btn.classList.add('processing');
        
        try {
            // Убираем active у всех кнопок в этой группе
            const buttons = control.querySelectorAll('.aggregation-btn');
            buttons.forEach(b => {
                b.classList.remove('active');
                b.disabled = false;
                b.classList.remove('processing');
            });
            
            // Добавляем active текущей кнопке
            btn.classList.add('active');
            
            // Обновляем активный интервал в data-атрибуте
            control.dataset.activeInterval = interval;
            
            // Получаем ID сенсора из родительского контейнера
            const sensorCard = control.closest('.bsp-chart-sensor-card');
            if (!sensorCard) {
                console.log('Sensor card not found');
                return;
            }
            
            const sensorId = sensorCard.dataset.sensorId;
            const sensorType = sensorCard.dataset.sensorType;
            
            console.log(`Sensor ID: ${sensorId}, Type: ${sensorType}`);
            
            // Получаем текущий выбранный период
            const timeRangeControl = document.querySelector(`.time-range-control[data-prefix="${prefix}"]`);
            let activeDays = 1;
            
            if (timeRangeControl) {
                activeDays = parseInt(timeRangeControl.dataset.activeDays || '1');
                console.log(`Found time range control with days: ${activeDays}`);
            } else {
                console.log(`Time range control not found for prefix: ${prefix}`);
            }
            
            // Обновляем отображение метода
            if (window.ChartsCommon) {
                console.log(`Updating method info: ${prefix}, ${sensorId}, ${interval}, ${activeDays}`);
                ChartsCommon.updateMethodInfo(prefix, sensorId, interval, activeDays);
                
                // Автоматически загружаем данные с новым интервалом
                console.log(`Loading test data...`);
                await ChartsCommon.loadTestData(prefix, sensorId, interval, activeDays);
            } else {
                console.error('ChartsCommon not found');
            }
            
            // Диспатчим событие об изменении интервала
            const event = new CustomEvent('aggregationIntervalChanged', {
                detail: {
                    prefix: prefix,
                    interval: interval,
                    sensorId: sensorId,
                    days: activeDays
                }
            });
            document.dispatchEvent(event);
            
            console.log(`[Aggregation] ${prefix}: interval changed to ${interval}, auto-loaded data`);
            
        } catch (error) {
            console.error(`[Aggregation] Error: ${error.message}`);
        } finally {
            // Разблокируем кнопку
            btn.disabled = false;
            btn.classList.remove('processing');
        }
    }
    
    // Запускаем инициализацию после загрузки DOM
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            console.log('DOM loaded, initializing aggregation controls');
            initAggregationControls();
        });
    } else {
        console.log('DOM already loaded, initializing aggregation controls now');
        initAggregationControls();
    }
    
    // Также инициализируем при динамической загрузке контента
    // Наблюдатель за изменениями в DOM
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.addedNodes.length) {
                // Проверяем, были ли добавлены новые кнопки агрегации
                const hasNewAggregationBtns = document.querySelectorAll('.aggregation-btn').length > 0;
                if (hasNewAggregationBtns) {
                    console.log('New aggregation buttons detected, reinitializing');
                    initAggregationControls();
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
    window.refreshAggregationControls = initAggregationControls;
})();