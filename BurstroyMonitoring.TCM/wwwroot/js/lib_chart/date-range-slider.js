// date-range-slider.js - Универсальный модуль для управления слайдером диапазона дат

const DateRangeSlider = (function() {
    'use strict';

    const instances = new Map();

    class SliderInstance {
        constructor(prefix, options = {}) {
            this.prefix = prefix;
            this.sliderElement = document.getElementById(`${prefix}DateRangeSlider`);
            this.container = $(`#${prefix}DateRangeSection`);
            this.sliderContainer = $(`#${prefix}SliderContainer`);
            this.dateRangeLabel = $(`#${prefix}DateRangeLabel`);
            this.minDateLabel = $(`#${prefix}MinDateLabel`);
            this.maxDateLabel = $(`#${prefix}MaxDateLabel`);

            this.slider = null;
            this.minDate = null;
            this.maxDate = null;
            this.measurements = [];
            this.onRangeChangeCallback = options.onRangeChange || null;
            this.initialized = false;

            if (!this.sliderElement) {
                console.error(`DateRangeSlider [${prefix}]: элемент слайдера не найден`);
                return;
            }
        }

        init(measurements) {
            if (typeof noUiSlider === 'undefined') {
                console.error('DateRangeSlider: noUiSlider не загружен');
                return false;
            }

            this.measurements = measurements || [];

            if (!this.measurements || this.measurements.length < 2) {
                this.container.addClass('disabled');
                this.sliderContainer.addClass('disabled');
                this.destroy();
                return false;
            }

            const timestamps = this.measurements
                .map(m => new Date(m.receivedAt).getTime())
                .filter(ts => !isNaN(ts));

            if (timestamps.length < 2) {
                this.container.addClass('disabled');
                this.sliderContainer.addClass('disabled');
                this.destroy();
                return false;
            }

            this.minDate = Math.min(...timestamps);
            this.maxDate = Math.max(...timestamps);

            if (isNaN(this.minDate) || isNaN(this.maxDate) || this.minDate >= this.maxDate) {
                console.error(`DateRangeSlider [${this.prefix}]: некорректные даты`);
                return false;
            }

            this.updateDateLabels(this.minDate, this.maxDate);

            this.container.removeClass('disabled');
            this.sliderContainer.removeClass('disabled');

            if (this.slider) {
                this.slider.updateOptions({
                    range: { min: this.minDate, max: this.maxDate },
                    start: [this.minDate, this.maxDate]
                });
                return true;
            }

            return this.createSlider();
        }

        createSlider() {
            this.sliderElement.innerHTML = '';

            setTimeout(() => {
                try {
                    this.slider = noUiSlider.create(this.sliderElement, {
                        start: [this.minDate, this.maxDate],
                        connect: true,
                        range: { min: this.minDate, max: this.maxDate },
                        step: 3600000, // 1 час
                        format: {
                            to: v => Math.round(v),
                            from: v => Math.round(v)
                        },
                        behaviour: 'tap-drag',
                        animate: true,
                        animationDuration: 300
                    });

                    this.slider.on('update', (values) => {
                        const start = parseInt(values[0]);
                        const end = parseInt(values[1]);
                        this.updateRangeLabel(start, end);
                    });

                    this.slider.on('start', () => {
                        $(document).trigger('sliderDragStart');
                    });

                    this.slider.on('end', (values) => {
                        const startTime = parseInt(values[0]);
                        const endTime = parseInt(values[1]);

                        if (this.onRangeChangeCallback) {
                            const filteredData = this.filterData(startTime, endTime);
                            this.onRangeChangeCallback(filteredData, startTime, endTime);
                        }

                        $(document).trigger('sliderDragEnd');
                    });

                    this.initialized = true;
                    console.log(`DateRangeSlider [${this.prefix}]: инициализирован`);

                } catch (e) {
                    console.error(`DateRangeSlider [${this.prefix}]: ошибка создания`, e);
                    this.initialized = false;
                }
            }, 50);

            return true;
        }

        filterData(startTime, endTime) {
            return this.measurements.filter(m => {
                const t = new Date(m.receivedAt).getTime();
                return t >= startTime && t <= endTime;
            });
        }

        updateDateLabels(min, max) {
            const formatDate = (ts) => moment(ts).format('DD.MM.YYYY HH:mm');
            this.minDateLabel.text(formatDate(min));
            this.maxDateLabel.text(formatDate(max));
        }

        updateRangeLabel(start, end) {
            const formatDate = (ts) => moment(ts).format('DD.MM.YYYY HH:mm');
            this.dateRangeLabel.text(`${formatDate(start)} - ${formatDate(end)}`);
        }

        destroy() {
            if (this.slider) {
                try {
                    this.slider.destroy();
                } catch (e) {
                    console.error(`DateRangeSlider [${this.prefix}]: ошибка при уничтожении`, e);
                }
                this.slider = null;
            }
            this.initialized = false;
        }

        isInitialized() {
            return this.initialized && this.slider !== null;
        }

        getCurrentRange() {
            if (!this.slider) return null;
            const values = this.slider.get();
            return {
                start: parseInt(values[0]),
                end: parseInt(values[1])
            };
        }

        setRange(start, end) {
            if (!this.slider) return;
            this.slider.set([start, end]);
        }

        resetToFullRange() {
            if (!this.slider || !this.minDate || !this.maxDate) return;
            this.setRange(this.minDate, this.maxDate);
        }
    }

    return {
        create: function(prefix, options = {}) {
            if (instances.has(prefix)) {
                console.warn(`DateRangeSlider: экземпляр с префиксом "${prefix}" уже существует. Уничтожаем старый.`);
                this.destroy(prefix);
            }

            const instance = new SliderInstance(prefix, options);
            instances.set(prefix, instance);
            return instance;
        },

        get: function(prefix) {
            return instances.get(prefix);
        },

        destroy: function(prefix) {
            const instance = instances.get(prefix);
            if (instance) {
                instance.destroy();
                instances.delete(prefix);
            }
        },

        destroyAll: function() {
            instances.forEach((instance, prefix) => {
                instance.destroy();
            });
            instances.clear();
        },

        initSlider: function(prefix, measurements) {
            const instance = instances.get(prefix);
            if (!instance) return false;
            return instance.init(measurements);
        }
    };
})();

$(document).ready(function() {
    console.log('✅ DateRangeSlider загружен');

    $(document).on('sensorChanged', function() {
        DateRangeSlider.destroyAll();
    });
});

window.DateRangeSlider = DateRangeSlider;