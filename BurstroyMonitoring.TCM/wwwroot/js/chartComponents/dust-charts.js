function initDUSTCharts(sensorId) {
    const container = $(`[data-sensor-id="${sensorId}"]`);
    let mainChart = null;
    let lastData = [];

    function loadData() {
        const interval = container.find('input[name="dustInterval"]:checked').val() || 'hour';
        const days = container.find('input[name="dustDays"]').val() || 1;

        let url = '/GraphsAndCharts/';
        if (interval === 'raw') url += 'GetDUSTData';
        else if (interval === 'ten-minute') url += 'GetDUSTDataTenMinuteInterval';
        else url += 'GetDUSTDataHour';

        $.get(url, { sensorId: sensorId, days: days }, function (response) {
            if (!response || !response.measurements) return;
            lastData = response.measurements;
            updateChart();
        });
    }

    function updateChart() {
        const selectedRadio = container.find('input[name="dustParam"]:checked');
        const parameter = selectedRadio.val();
        const chartType = container.find('input[name="dustChartType"]:checked').val() || 'line';
        const canvasId = `dust-main-chart-${sensorId}`;
        const ctx = document.getElementById(canvasId);

        if (!ctx || lastData.length === 0) return;

        if (parameter === 'pm_all') {
            renderMultiChart(ctx, chartType);
        } else {
            const parameterName = selectedRadio.next('label').text();
            if (mainChart) {
                ChartUtils.updateChartData(mainChart, lastData, 'receivedAt', parameter, chartType);
                mainChart.data.datasets[0].label = parameterName;
                mainChart.update();
            } else {
                mainChart = ChartUtils.createChart(ctx, parameterName, lastData, 'receivedAt', parameter, chartType);
            }
        }
    }

    function renderMultiChart(ctx, chartType) {
        const datasets = [
            { label: 'PM10 (актуальное)', field: 'pm10Act', color: '#ff6384' },
            { label: 'PM10 (среднее)', field: 'pm10Awg', color: '#ff9f40' },
            { label: 'PM2.5 (актуальное)', field: 'pm25Act', color: '#36a2eb' },
            { label: 'PM2.5 (среднее)', field: 'pm25Awg', color: '#4bc0c0' },
            { label: 'PM1.0 (актуальное)', field: 'pm1Act', color: '#9966ff' },
            { label: 'PM1.0 (среднее)', field: 'pm1Awg', color: '#ffcd56' }
        ];

        const config = {
            type: chartType === 'scatter' ? 'scatter' : 'line',
            data: {
                datasets: datasets.map(ds => ({
                    label: ds.label,
                    data: lastData.map(item => ({
                        x: new Date(item.receivedAt),
                        y: item[ds.field]
                    })),
                    borderColor: ds.color,
                    backgroundColor: ds.color + '33',
                    borderWidth: 2,
                    pointRadius: chartType === 'scatter' ? 4 : 2,
                    showLine: chartType !== 'scatter',
                    tension: 0.1
                }))
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: { type: 'time', time: { unit: 'hour' }, title: { display: true, text: 'Время' } },
                    y: { beginAtZero: true, title: { display: true, text: 'Концентрация' } }
                }
            }
        };

        if (mainChart) mainChart.destroy();
        mainChart = new Chart(ctx, config);
    }

    // Обработчики событий
    ChartUtils.initRangeButtons(container, loadData);
    container.find('input[name="dustInterval"]').on('change', loadData);
    container.find('.range-value-input').on('change', loadData);
    container.find('input[name="dustParam"], input[name="dustChartType"]').on('change', updateChart);
    
    // Автообновление
    container.find('#auto-update-dust').on('change', function() {
        AutoUpdate.toggle(`dust-${sensorId}`, $(this).is(':checked'), loadData, 60);
    });

    // Начальная загрузка
    loadData();
}
