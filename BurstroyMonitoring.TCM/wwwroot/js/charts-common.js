// Общие утилиты для работы с графиками
const ChartsCommon = (function() {
    'use strict';
    
    // Карта соответствия типов датчиков и методов
    const methodMap = {
        dov: {
            raw: 'GetDOVData',
            tenminutes: 'GetDOVDataTenMinuteInterval',
            hour: 'GetDOVDataHour'
        },
        dspd: {
            raw: 'GetDSPDData',
            tenminutes: 'GetDSPDDataTenMinuteInterval',
            hour: 'GetDSPDDataHour'
        },
        dust: {
            raw: 'GetDUSTData',
            tenminutes: 'GetDUSTDataTenMinuteInterval',
            hour: 'GetDUSTDataHour'
        },
        iws: {
            raw: 'GetIWSData',
            tenminutes: 'GetIWSDataTenMinuteInterval',
            hour: 'GetIWSDataHour'
        },
        mueks: {
            raw: 'GetMUEKSData',
            tenminutes: 'GetMUEKSDataTenMinuteInterval',
            hour: 'GetMUEKSDataHour'
        }
    };
    
    // Получить название метода для датчика
    function getMethodName(sensorType, interval) {
        if (!methodMap[sensorType] || !methodMap[sensorType][interval]) {
            console.warn(`Unknown sensor type or interval: ${sensorType}, ${interval}`);
            return null;
        }
        return methodMap[sensorType][interval];
    }
    
    // Построить URL для запроса данных
    function buildDataUrl(sensorType, interval, sensorId, days) {
        const methodName = getMethodName(sensorType, interval);
        if (!methodName) return null;
        
        return `/GraphsAndCharts/${methodName}?sensorId=${sensorId}&days=${days}`;
    }
    
    // Обновить информацию о методе в отладочной панели
    function updateMethodInfo(prefix, sensorId, interval, days) {
        const methodNameSpan = document.getElementById(`${prefix}MethodName`);
        const paramsSpan = document.getElementById(`${prefix}MethodParams`);
        const urlSpan = document.getElementById(`${prefix}MethodUrl`);
        
        if (!methodNameSpan || !paramsSpan || !urlSpan) return;
        
        const sensorType = prefix; // prefix совпадает с типом датчика
        const methodName = getMethodName(sensorType, interval);
        
        if (methodName) {
            methodNameSpan.textContent = methodName;
            paramsSpan.textContent = `sensorId=${sensorId}, days=${days}`;
            urlSpan.textContent = `/GraphsAndCharts/${methodName}?sensorId=${sensorId}&days=${days}`;
            
            // Добавляем класс для выделения
            const methodInfo = document.getElementById(`${prefix}MethodInfo`);
            if (methodInfo) {
                methodInfo.style.borderLeftColor = getStatusColor('ready');
            }
        }
    }
    
    // Получить цвет статуса
    function getStatusColor(status) {
        switch(status) {
            case 'loading': return '#ffc107';
            case 'success': return '#28a745';
            case 'error': return '#dc3545';
            default: return '#6c757d';
        }
    }
    
    // Показать индикатор загрузки
    function showLoading(prefix) {
        const statusSpan = document.getElementById(`${prefix}MethodStatus`);
        if (statusSpan) {
            statusSpan.innerHTML = 'Загрузка... <span class="loading-indicator"></span>';
            statusSpan.style.color = '#ffc107';
        }
    }
    
    // Обновить статус
    function updateStatus(prefix, status, message) {
        const statusSpan = document.getElementById(`${prefix}MethodStatus`);
        if (!statusSpan) return;
        
        const methodInfo = document.getElementById(`${prefix}MethodInfo`);
        
        switch(status) {
            case 'success':
                statusSpan.textContent = message || 'Успешно загружено';
                statusSpan.style.color = '#28a745';
                if (methodInfo) methodInfo.style.borderLeftColor = '#28a745';
                break;
            case 'error':
                statusSpan.textContent = message || 'Ошибка загрузки';
                statusSpan.style.color = '#dc3545';
                if (methodInfo) methodInfo.style.borderLeftColor = '#dc3545';
                break;
            case 'loading':
                statusSpan.innerHTML = 'Загрузка... <span class="loading-indicator"></span>';
                statusSpan.style.color = '#ffc107';
                if (methodInfo) methodInfo.style.borderLeftColor = '#ffc107';
                break;
            default:
                statusSpan.textContent = message || 'Готов к загрузке';
                statusSpan.style.color = '#6c757d';
                if (methodInfo) methodInfo.style.borderLeftColor = '#6c757d';
        }
    }
    
    // Загрузить данные и показать результат
    async function loadTestData(prefix, sensorId, interval, days) {
        const url = buildDataUrl(prefix, interval, sensorId, days);
        if (!url) {
            alert('Не удалось построить URL для запроса');
            return Promise.reject('No URL');
        }
        
        const resultDiv = document.getElementById(`${prefix}TestResult`);
        const preElement = resultDiv?.querySelector('pre');
        
        if (!resultDiv || !preElement) {
            return Promise.reject('Result container not found');
        }
        
        // Показываем результат и индикатор загрузки
        resultDiv.style.display = 'block';
        preElement.textContent = 'Загрузка данных...';
        updateStatus(prefix, 'loading');
        
        try {
            const response = await fetch(url);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            
            // Форматируем JSON для отображения
            preElement.textContent = JSON.stringify(data, null, 2);
            
            // Обновляем бейдж с количеством записей
            const badge = document.getElementById(`${prefix}ResultBadge`);
            if (badge) {
                badge.textContent = `Записей: ${data.measurements?.length || 0}`;
                badge.style.background = '#28a745';
            }
            
            updateStatus(prefix, 'success', `Загружено ${data.measurements?.length || 0} записей`);
            return data;
            
        } catch (error) {
            console.error('Error loading data:', error);
            preElement.textContent = `Ошибка загрузки: ${error.message}`;
            
            const badge = document.getElementById(`${prefix}ResultBadge`);
            if (badge) {
                badge.textContent = 'Ошибка';
                badge.style.background = '#dc3545';
            }
            
            updateStatus(prefix, 'error', error.message);
            throw error;
        }
    }


    function clearResult(prefix) {
        const resultDiv = document.getElementById(`${prefix}TestResult`);
        if (resultDiv) {
            resultDiv.style.display = 'none';
            const preElement = resultDiv.querySelector('pre');
            if (preElement) {
                preElement.textContent = '';
            }
        }
        
        // Сбрасываем статус
        updateStatus(prefix, 'ready', 'Готов к загрузке');
        
        // Возвращаем метод по умолчанию
        const sensorCard = document.querySelector(`.bsp-chart-sensor-card[data-sensor-type="${prefix}"]`);
        if (sensorCard) {
            const sensorId = sensorCard.dataset.sensorId;
            const aggregationControl = document.querySelector(`.aggregation-interval-control[data-prefix="${prefix}"]`);
            const timeRangeControl = document.querySelector(`.time-range-control[data-prefix="${prefix}"]`);
            
            const interval = aggregationControl ? aggregationControl.dataset.activeInterval || 'hour' : 'hour';
            const days = timeRangeControl ? parseInt(timeRangeControl.dataset.activeDays || '1') : 1;
            
            updateMethodInfo(prefix, sensorId, interval, days);
        }
    }



    // Публичный API
    return {
        getMethodName,
        buildDataUrl,
        updateMethodInfo,
        updateStatus,
        showLoading,
        loadTestData,
         clearResult
    };
})();

// Экспорт для использования в других файлах
window.ChartsCommon = ChartsCommon;