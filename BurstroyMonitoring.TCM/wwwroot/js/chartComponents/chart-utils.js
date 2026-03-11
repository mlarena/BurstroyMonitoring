const ChartUtils = {
    // Цвета для графиков
    colors: [
        '#36a2eb', '#ff6384', '#4bc0c0', '#ff9f40', '#9966ff', '#ffcd56', '#c9cbcf'
    ],

    // Инициализация графика
    createChart: (ctx, label, data, xField, yField, chartType = 'line') => {
        const config = {
            type: chartType === 'scatter' ? 'scatter' : 'line',
            data: {
                datasets: [{
                    label: label,
                    data: data.map(item => ({
                        x: new Date(item[xField]),
                        y: item[yField]
                    })),
                    borderColor: ChartUtils.colors[0],
                    backgroundColor: ChartUtils.colors[0] + '33',
                    borderWidth: 2,
                    pointRadius: chartType === 'scatter' ? 4 : 2,
                    showLine: chartType !== 'scatter',
                    tension: 0.1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: 'hour',
                            displayFormats: {
                                hour: 'HH:mm',
                                day: 'MMM d'
                            }
                        },
                        title: { display: true, text: 'Время' }
                    },
                    y: {
                        beginAtZero: false,
                        title: { display: true, text: 'Значение' }
                    }
                },
                plugins: {
                    zoom: { // Если подключен плагин zoom
                        zoom: { wheel: { enabled: true }, mode: 'x' },
                        pan: { enabled: true, mode: 'x' }
                    }
                }
            }
        };
        return new Chart(ctx, config);
    },

    // Обновление данных в существующем графике
    updateChartData: (chart, newData, xField, yField, chartType) => {
        chart.config.type = chartType === 'scatter' ? 'scatter' : 'line';
        chart.data.datasets[0].data = newData.map(item => ({
            x: new Date(item[xField]),
            y: item[yField]
        }));
        chart.data.datasets[0].showLine = chartType !== 'scatter';
        chart.data.datasets[0].pointRadius = chartType === 'scatter' ? '4' : '2';
        chart.update();
    },

    // Инициализация обработчиков для кнопок периода
    initRangeButtons: (container, callback) => {
        container.find('.range-btn').on('click', function() {
            const btn = $(this);
            const days = btn.data('days');
            const parent = btn.closest('.time-range-control');
            
            // Обновляем активный класс
            parent.find('.range-btn').removeClass('active');
            btn.addClass('active');
            
            // Обновляем скрытый инпут и вызываем событие
            parent.find('.range-value-input').val(days).trigger('change');
        });
    }
};
