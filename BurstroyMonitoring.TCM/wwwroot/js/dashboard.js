/**
 * dashboard.js
 * Ядро системы веб-частей для BurstroyMonitoring.TCM.
 */

let grid = null;

const DashboardCore = {
    init: function () {
        this.initGridStack();
        this.setupEvents();
        setTimeout(() => this.loadAllWebParts(), 200);
    },

    initGridStack: function () {
        if (!$('#dashboard-grid').length) return;

        grid = GridStack.init({
            cellHeight: '120px',
            verticalMargin: 15,
            minRow: 3,
            staticGrid: true,
            draggable: { handle: '.card-header', scroll: true, appendTo: 'body' },
            resizable: { handles: 'e, se, s, sw, w' },
            animate: true
        });

        $('#editModeSwitch').on('change', function () {
            const isEdit = $(this).is(':checked');
            if (grid) grid.setStatic(!isEdit);
            $('.webpart').toggleClass('edit-mode', isEdit);
        });

        grid.on('change', (event, items) => {
            if (!items || !Array.isArray(items)) return;
            items.forEach(item => {
                if (item.id) {
                    this.api.updatePosition(item.id, item.x, item.y);
                    this.api.updateSize(item.id, item.w, item.h);
                }
            });
        });
    },

    setupEvents: function () {
        $(document).ajaxError((event, xhr, settings, error) => {
            console.error('AJAX Error:', error, settings.url);
        });
    },

    loadAllWebParts: function () {
        $('.grid-stack-item[gs-id]').each((i, el) => {
            const id = $(el).attr('gs-id');
            if (id) this.loadWebPartContent(parseInt(id));
        });
    },

    loadWebPartContent: function (id) {
        const $card = $(`#webpart-${id}`);
        const type = $card.attr('data-type');
        const $container = $card.find('.webpart-content');
        if (!type) return;

        const urlMap = {
            'MonitoringPosts':  '/WebParts/GetMonitoringPostsWebPart',
            'MonitoringMap':    '/WebParts/GetMonitoringMapWebPart',
            'SensorData':       '/WebParts/GetSensorDataWebPart',
            'GraphsAndCharts':  '/WebParts/GetGraphsAndChartsWebPart',
            'Report':           '/WebParts/GetReportWebPart',
            'Cameras':          '/WebParts/GetCamerasWebPart'
        };

        const url = urlMap[type];
        if (!url) {
            $container.html('<div class="alert alert-warning small">Неизвестный тип: ' + type + '</div>');
            return;
        }

        $container.html('<div class="text-center my-auto"><div class="spinner-border spinner-border-sm text-primary"></div></div>');

        $.get(url)
            .done(html => $container.html(html))
            .fail(() => $container.html('<div class="alert alert-danger small">Ошибка загрузки ' + type + '</div>'));
    },

    api: {
        add: function (type, title) {
            $.post('/Dashboard/AddWebPart', { type: type, title: title || '' }, html => {
                const $card = $(html);
                const w = parseInt($card.attr('gs-w')) || 6;
                const h = parseInt($card.attr('gs-h')) || 4;
                const id = $card.attr('gs-id');

                // Оборачиваем в правильную GridStack-структуру
                const $item = $('<div class="grid-stack-item"><div class="grid-stack-item-content"></div></div>');
                $item.attr('gs-id', id);
                $item.find('.grid-stack-item-content').append($card);

                const el = grid.addWidget($item[0], { id: id, w: w, h: h });
                if (id) DashboardCore.loadWebPartContent(parseInt(id));
            });
        },
        remove: function (id) {
            if (confirm('Удалить веб-часть?')) {
                $.post('/Dashboard/RemoveWebPart', { webPartId: id }, () => {
                    grid.removeWidget($(`.grid-stack-item[gs-id="${id}"]`)[0]);
                });
            }
        },
        updatePosition: (id, x, y) => $.post('/Dashboard/UpdateWebPartPosition', { webPartId: id, x: x, y: y }),
        updateSize:     (id, w, h) => $.post('/Dashboard/UpdateWebPartSize',     { webPartId: id, width: w, height: h })
    }
};

// Глобальные алиасы для onclick в шаблонах
window.addWebPart    = (type, title) => DashboardCore.api.add(type, title);
window.removeWebPart = (id)          => DashboardCore.api.remove(id);
window.refreshWebPart = (id)         => DashboardCore.loadWebPartContent(id);
window.editWebPartTitle = (id) => {
    const newTitle = prompt('Введите новый заголовок:');
    if (newTitle) {
        $.post('/Dashboard/UpdateWebPartTitle', { webPartId: id, title: newTitle }, () => {
            $(`#webpart-${id} .webpart-title-text`).text(newTitle);
        });
    }
};
