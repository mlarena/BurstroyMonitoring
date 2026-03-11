const AutoUpdate = {
    intervals: {}, // Храним интервалы для разных графиков

    // Запуск автообновления
    start: (id, callback, seconds = 60) => {
        if (AutoUpdate.intervals[id]) {
            clearInterval(AutoUpdate.intervals[id]);
        }
        AutoUpdate.intervals[id] = setInterval(callback, seconds * 1000);
        console.log(`Auto-update started for ${id} every ${seconds}s`);
    },

    // Остановка автообновления
    stop: (id) => {
        if (AutoUpdate.intervals[id]) {
            clearInterval(AutoUpdate.intervals[id]);
            delete AutoUpdate.intervals[id];
            console.log(`Auto-update stopped for ${id}`);
        }
    },

    // Переключение состояния (toggle)
    toggle: (id, isEnabled, callback, seconds = 60) => {
        if (isEnabled) {
            AutoUpdate.start(id, callback, seconds);
        } else {
            AutoUpdate.stop(id);
        }
    }
};
