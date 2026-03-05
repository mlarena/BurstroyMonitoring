// Управление временными диапазонами
(function() {
    'use strict';
    
    document.addEventListener('DOMContentLoaded', function() {
        initTimeRangeControls();
    });
    
    function initTimeRangeControls() {
        // Находим все контролы временных диапазонов
        const timeRangeControls = document.querySelectorAll('.time-range-control');
        
        timeRangeControls.forEach(control => {
            const prefix = control.dataset.prefix;
            const buttons = control.querySelectorAll('.range-btn');
            
            buttons.forEach(btn => {
                btn.addEventListener('click', function(e) {
                    e.preventDefault();
                    
                    const days = parseInt(this.dataset.days);
                    
                    // Убираем active у всех кнопок в этой группе
                    buttons.forEach(b => b.classList.remove('active'));
                    
                    // Добавляем active текущей кнопке
                    this.classList.add('active');
                    
                    // Обновляем активный период в data-атрибуте
                    control.dataset.activeDays = days;
                    
                    // Получаем ID сенсора из родительского контейнера
                    const sensorCard = control.closest('.bsp-chart-sensor-card');
                    if (!sensorCard) return;
                    
                    const sensorId = sensorCard.dataset.sensorId;
                    const sensorType = sensorCard.dataset.sensorType;
                    
                    // Получаем текущий выбранный интервал агрегации
                    const aggregationControl = document.querySelector(`.aggregation-interval-control[data-prefix="${prefix}"]`);
                    const activeInterval = aggregationControl ? 
                        aggregationControl.dataset.activeInterval || 'hour' : 'hour';
                    
                    // Обновляем отображение метода
                    if (window.ChartsCommon) {
                        ChartsCommon.updateMethodInfo(prefix, sensorId, activeInterval, days);
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
                    
                    console.log(`[TimeRange] ${prefix}: days changed to ${days}`);
                });
            });
        });
    }
})();