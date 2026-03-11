function initDOVCharts(sensorId) {
    const container = $(`[data-sensor-id="${sensorId}"]`);
    let mainChart = null;
    let lastData = [];

    function loadData() {
        const interval = container.find('input[name="dovInterval"]:checked').val() || 'hour';
        const days = container.find('input[name="dovDays"]').val() || 1;

        let url = '/GraphsAndCharts/';
        if (interval === 'raw') url += 'GetDOVData';
        else if (interval === 'ten-minute') url += 'GetDOVDataTenMinuteInterval';
        else url += 'GetDOVDataHour';

        $.get(url, { sensorId: sensorId, days: days }, function (response) {
            if (!response || !response.measurements) return;
            lastData = response.measurements;
            updateChart();
        });
    }

    function updateChart() {
        const chartType = container.find('input[name="dovChartStyle"]:checked').val() || 'line';
        const canvasId = `dov-main-chart-${sensorId}`;
        const ctx = document.getElementById(canvasId);

        if (!ctx || lastData.length === 0) return;

        if (mainChart) {
            ChartUtils.updateChartData(mainChart, lastData, 'receivedAt', 'visibleRange', chartType);
            mainChart.update();
        } else {
            mainChart = ChartUtils.createChart(ctx, 'Видимость (м)', lastData, 'receivedAt', 'visibleRange', chartType);
        }
    }

    // Обработчики событий
    ChartUtils.initRangeButtons(container, loadData);
    container.find('input[name="dovInterval"]').on('change', loadData);
    container.find('.range-value-input').on('change', loadData);
    container.find('input[name="dovChartStyle"]').on('change', updateChart);
    
    // Автообновление
    container.find('#auto-update-dov').on('change', function() {
        AutoUpdate.toggle(`dov-${sensorId}`, $(this).is(':checked'), loadData, 60);
    });

    // Начальная загрузка
    loadData();
}
