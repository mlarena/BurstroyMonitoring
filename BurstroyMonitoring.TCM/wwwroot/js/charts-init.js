// Инициализация для каждого типа датчика
(function() {
    'use strict';
    
    document.addEventListener('DOMContentLoaded', function() {
        initTestButtons();
        initSensorCards();
    });
    
    function initTestButtons() {
        // Используем делегирование событий, но с защитой от двойных вызовов
        document.addEventListener('click', function(e) {
            // Находим кнопку тестовой загрузки
            const testBtn = e.target.closest('.test-load-btn');
            if (testBtn) {
                e.preventDefault();
                e.stopPropagation(); // Останавливаем всплытие события
                
                // Проверяем, не обрабатывается ли уже этот клик
                if (testBtn.disabled || testBtn.classList.contains('processing')) {
                    return;
                }
                
                // Блокируем кнопку на время обработки
                testBtn.disabled = true;
                testBtn.classList.add('processing');
                
                const sensorCard = testBtn.closest('.bsp-chart-sensor-card');
                if (!sensorCard) {
                    testBtn.disabled = false;
                    testBtn.classList.remove('processing');
                    return;
                }
                
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
                    ChartsCommon.loadTestData(prefix, sensorId, interval, days)
                        .finally(() => {
                            // Разблокируем кнопку после завершения
                            testBtn.disabled = false;
                            testBtn.classList.remove('processing');
                        });
                } else {
                    testBtn.disabled = false;
                    testBtn.classList.remove('processing');
                }
            }
        });
        
        // Отдельный обработчик для кнопок очистки
        document.addEventListener('click', function(e) {
            const clearBtn = e.target.closest('.clear-result-btn');
            if (clearBtn) {
                e.preventDefault();
                e.stopPropagation(); // Останавливаем всплытие события
                
                // Проверяем блокировку
                if (clearBtn.disabled || clearBtn.classList.contains('processing')) {
                    return;
                }
                
                clearBtn.disabled = true;
                clearBtn.classList.add('processing');
                
                const prefix = clearBtn.dataset.prefix;
                if (window.ChartsCommon) {
                    ChartsCommon.clearResult(prefix);
                }
                
                // Разблокируем после небольшой задержки
                setTimeout(() => {
                    clearBtn.disabled = false;
                    clearBtn.classList.remove('processing');
                }, 300);
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