function initMUEKSCharts(sensorId) {
    const container = $(`[data-sensor-id="${sensorId}"]`);
    let mainChart = null;
    let lastData = [];

    function loadData() {
        const interval = container.find('input[name="mueksInterval"]:checked').val() || 'hour';
        const days = container.find('input[name="mueksDays"]').val() || 1;

        let url = '/GraphsAndCharts/';
        if (interval === 'raw') url += 'GetMUEKSData';
        else if (interval === 'ten-minute') url += 'GetMUEKSDataTenMinuteInterval';
        else url += 'GetMUEKSDataHour';

        $.get(url, { sensorId: sensorId, days: days }, function (response) {
            if (!response || !response.measurements) return;
            lastData = response.measurements;
            updateChart();
        });
    }

    function updateChart() {
        const selectedRadio = container.find('input[name="mueksParam"]:checked');
        const parameter = selectedRadio.val();
        const parameterName = selectedRadio.next('label').text();
        const chartType = container.find('input[name="mueksChartType"]:checked').val() || 'line';
        const canvasId = `mueks-main-chart-${sensorId}`;
        const ctx = document.getElementById(canvasId);

        if (!ctx || lastData.length === 0) return;

        if (mainChart) {
            ChartUtils.updateChartData(mainChart, lastData, 'receivedAt', parameter, chartType);
            mainChart.data.datasets[0].label = parameterName;
            mainChart.update();
        } else {
            mainChart = ChartUtils.createChart(ctx, parameterName, lastData, 'receivedAt', parameter, chartType);
        }
    }

    // Обработчики событий
    ChartUtils.initRangeButtons(container, loadData);
    container.find('input[name="mueksInterval"]').on('change', loadData);
    container.find('.range-value-input').on('change', loadData);
    container.find('input[name="mueksParam"], input[name="mueksChartType"]').on('change', updateChart);
    
    // Автообновление
    container.find('#auto-update-mueks').on('change', function() {
        AutoUpdate.toggle(`mueks-${sensorId}`, $(this).is(':checked'), loadData, 60);
    });

    // Начальная загрузка
    loadData();
}
