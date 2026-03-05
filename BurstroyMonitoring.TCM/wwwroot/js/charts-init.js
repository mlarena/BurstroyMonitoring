// Инициализация для каждого типа датчика
(function() {
    'use strict';
    
    document.addEventListener('DOMContentLoaded', function() {
        initTestButtons();
        initSensorCards();
    });
    
    function initTestButtons() {
        // Обработчики для тестовых кнопок
        document.addEventListener('click', function(e) {
            const testBtn = e.target.closest('.test-load-btn');
            if (!testBtn) return;
            
            e.preventDefault();
            
            const sensorCard = testBtn.closest('.bsp-chart-sensor-card');
            if (!sensorCard) return;
            
            const sensorId = sensorCard.dataset.sensorId;
            const sensorType = sensorCard.dataset.sensorType;
            const prefix = sensorType; // prefix совпадает с типом
            
            // Получаем текущие настройки
            const aggregationControl = document.querySelector(`.aggregation-interval-control[data-prefix="${prefix}"]`);
            const timeRangeControl = document.querySelector(`.time-range-control[data-prefix="${prefix}"]`);
            
            const interval = aggregationControl ? 
                aggregationControl.dataset.activeInterval || 'hour' : 'hour';
            const days = timeRangeControl ? 
                parseInt(timeRangeControl.dataset.activeDays || '1') : 1;
            
            // Загружаем тестовые данные
            if (window.ChartsCommon) {
                ChartsCommon.loadTestData(prefix, sensorId, interval, days);
            }
        });
         document.addEventListener('click', function(e) {
        const testBtn = e.target.closest('.test-load-btn');
        if (testBtn) {
            e.preventDefault();
            
            const sensorCard = testBtn.closest('.bsp-chart-sensor-card');
            if (!sensorCard) return;
            
            const sensorId = sensorCard.dataset.sensorId;
            const sensorType = sensorCard.dataset.sensorType;
            const prefix = sensorType;
            
            // Получаем текущие настройки
            const aggregationControl = document.querySelector(`.aggregation-interval-control[data-prefix="${prefix}"]`);
            const timeRangeControl = document.querySelector(`.time-range-control[data-prefix="${prefix}"]`);
            
            const interval = aggregationControl ? 
                aggregationControl.dataset.activeInterval || 'hour' : 'hour';
            const days = timeRangeControl ? 
                parseInt(timeRangeControl.dataset.activeDays || '1') : 1;
            
            // Загружаем тестовые данные
            if (window.ChartsCommon) {
                ChartsCommon.loadTestData(prefix, sensorId, interval, days);
            }
        }
        
        // Обработчик для кнопок очистки
        const clearBtn = e.target.closest('.clear-result-btn');
        if (clearBtn) {
            e.preventDefault();
            const prefix = clearBtn.dataset.prefix;
            if (window.ChartsCommon) {
                ChartsCommon.clearResult(prefix);
            }
        }
    });
    }
    
    function initSensorCards() {
        // Инициализация каждой карточки датчика при загрузке
        const sensorCards = document.querySelectorAll('.bsp-chart-sensor-card');
        
        sensorCards.forEach(card => {
            const sensorId = card.dataset.sensorId;
            const sensorType = card.dataset.sensorType;
            const prefix = sensorType;
            
            // Получаем текущие настройки
            const aggregationControl = document.querySelector(`.aggregation-interval-control[data-prefix="${prefix}"]`);
            const timeRangeControl = document.querySelector(`.time-range-control[data-prefix="${prefix}"]`);
            
            const interval = aggregationControl ? 
                aggregationControl.dataset.activeInterval || 'hour' : 'hour';
            const days = timeRangeControl ? 
                parseInt(timeRangeControl.dataset.activeDays || '1') : 1;
            
            // Обновляем информацию о методе
            if (window.ChartsCommon) {
                ChartsCommon.updateMethodInfo(prefix, sensorId, interval, days);
            }
            
            console.log(`[Init] ${prefix} card initialized with sensorId=${sensorId}, interval=${interval}, days=${days}`);
        });
    }
    
    // Слушаем события изменений для логирования
    document.addEventListener('aggregationIntervalChanged', function(e) {
        console.log('Aggregation interval changed:', e.detail);
    });
    
    document.addEventListener('timeRangeChanged', function(e) {
        console.log('Time range changed:', e.detail);
    });
})();