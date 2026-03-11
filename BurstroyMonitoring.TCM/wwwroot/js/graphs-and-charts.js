$(document).ready(function () {
    const postSelect = $('#monitoringPostSelect');
    const sensorSelect = $('#sensorSelect');
    const chartContainer = $('#chartContainer');

    // При выборе поста загружаем список датчиков
    postSelect.on('change', function () {
        const postId = $(this).val();
        if (!postId) {
            sensorSelect.html('<option value="">Выберите датчик</option>').prop('disabled', true);
            chartContainer.empty();
            return;
        }

        $.get('/GraphsAndCharts/GetSensorsByPost', { monitoringPostId: postId }, function (sensors) {
            let options = '<option value="">Выберите датчик</option>';
            sensors.forEach(sensor => {
                // В ответе приходят объекты SelectListItem (Value и Text)
                const id = sensor.value || sensor.Value;
                const text = sensor.text || sensor.Text;
                options += `<option value="${id}">${text}</option>`;
            });
            sensorSelect.html(options).prop('disabled', false);
            chartContainer.empty();
        });
    });

    // При выборе датчика загружаем его частичное представление
    sensorSelect.on('change', function () {
        const sensorId = $(this).val();
        if (!sensorId) {
            chartContainer.empty();
            return;
        }

        chartContainer.html('<div class="text-center p-5"><div class="spinner-border text-primary" role="status"></div><p class="mt-2">Загрузка графиков...</p></div>');

        $.get('/GraphsAndCharts/GetSensorData', { sensorId: sensorId }, function (html) {
            chartContainer.html(html);
        }).fail(function() {
            chartContainer.html('<div class="alert alert-danger">Ошибка при загрузке данных датчика</div>');
        });
    });
});
