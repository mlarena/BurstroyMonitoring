// Управление интервалами агрегации
(function() {
    'use strict';
    
    document.addEventListener('DOMContentLoaded', function() {
        initAggregationControls();
    });
    
    function initAggregationControls() {
        // Находим все контролы интервалов агрегации
        const aggregationControls = document.querySelectorAll('.aggregation-interval-control');
        
        aggregationControls.forEach(control => {
            const prefix = control.dataset.prefix;
            const buttons = control.querySelectorAll('.interval-btn');
            
            buttons.forEach(btn => {
                btn.addEventListener('click', function(e) {
                    e.preventDefault();
                    
                    const interval = this.dataset.interval;
                    
                    // Убираем active у всех кнопок в этой группе
                    buttons.forEach(b => b.classList.remove('active'));
                    
                    // Добавляем active текущей кнопке
                    this.classList.add('active');
                    
                    // Обновляем активный интервал в data-атрибуте
                    control.dataset.activeInterval = interval;
                    
                    // Получаем ID сенсора из родительского контейнера
                    const sensorCard = control.closest('.bsp-chart-sensor-card');
                    if (!sensorCard) return;
                    
                    const sensorId = sensorCard.dataset.sensorId;
                    const sensorType = sensorCard.dataset.sensorType;
                    
                    // Получаем текущий выбранный период
                    const timeRangeControl = document.querySelector(`.time-range-control[data-prefix="${prefix}"]`);
                    const activeDays = timeRangeControl ? 
                        parseInt(timeRangeControl.dataset.activeDays || '1') : 1;
                    
                    // Обновляем отображение метода
                    if (window.ChartsCommon) {
                        ChartsCommon.updateMethodInfo(prefix, sensorId, interval, activeDays);
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
                    
                    console.log(`[Aggregation] ${prefix}: interval changed to ${interval}`);
                });
            });
        });
    }
})();