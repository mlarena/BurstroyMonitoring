/**
 * Конфигурация метрик для различных типов сенсоров
 */
const metricsGroups = {
    "DOV": {
        "visibleRange": { "name": "Дальность видимости", "unit": "м", "color": "#1f77b4", "axis": "left" }
    },
    "DSPD": {
        "gripCoefficient": { "name": "Коэффициент сцепления", "unit": "", "color": "#d62728", "axis": "right" },
        "shakeLevel": { "name": "Уровень вибрации", "unit": "g", "color": "#ff7f0e", "axis": "right2" },
        "voltagePower": { "name": "Напряжение питания", "unit": "В", "color": "#2ca02c", "axis": "left" },
        "caseTemperature": { "name": "Температура внутри корпуса", "unit": "°C", "color": "#9467bd", "axis": "left" },
        "roadTemperature": { "name": "Температура дорожного покрытия", "unit": "°C", "color": "#8c564b", "axis": "left" },
        "waterHeight": { "name": "Высота слоя воды", "unit": "мм", "color": "#e377c2", "axis": "right" },
        "iceHeight": { "name": "Высота слоя льда", "unit": "мм", "color": "#7f7f7f", "axis": "right2" },
        "snowHeight": { "name": "Высота слоя снега", "unit": "мм", "color": "#bcbd22", "axis": "right2" },
        "icePercentage": { "name": "Процент обледенения", "unit": "%", "color": "#17becf", "axis": "right" },
        "pgmPercentage": { "name": "Процент реагента", "unit": "%", "color": "#aec7e8", "axis": "right" },
        "roadStatusCode": { "name": "Код состояния дороги", "unit": "", "color": "#ffbb78", "axis": "right2" },
        // "roadAngle": { "name": "Угол наклона дороги", "unit": "°", "color": "#98df8a", "axis": "right2" },
        // "freezeTemperature": { "name": "Температура замерзания", "unit": "°C", "color": "#ff9896", "axis": "left" },
        // "distanceToSurface": { "name": "Расстояние до поверхности", "unit": "мм", "color": "#c5b0d5", "axis": "right" }
    },
    "DUST": {
        "pm10Act": { "name": "Концентрация PM10", "unit": "мкг/м³", "color": "#1f77b4", "axis": "left" },
        "pm25Act": { "name": "Концентрация PM2.5", "unit": "мкг/м³", "color": "#ff7f0e", "axis": "left" },
        "pm1Act": { "name": "Концентрация PM1", "unit": "мкг/м³", "color": "#2ca02c", "axis": "left" },
        "pm10Awg": { "name": "Концентрация PM10 средняя", "unit": "мкг/м³", "color": "#d62728", "axis": "left" },
        "pm25Awg": { "name": "Концентрация PM2.5 средняя", "unit": "мкг/м³", "color": "#9467bd", "axis": "left" },
        "pm1Awg": { "name": "Концентрация PM1 средняя", "unit": "мкг/м³", "color": "#8c564b", "axis": "left" },
        "flowProbe": { "name": "Расход воздуха", "unit": "л/мин", "color": "#e377c2", "axis": "right" }
        // ,
        // "temperatureProbe": { "name": "Температура пробоотборника", "unit": "°C", "color": "#7f7f7f", "axis": "right2" },
        // "humidityProbe": { "name": "Влажность пробоотборника", "unit": "%", "color": "#bcbd22", "axis": "right" }
    },
    "MUEKS": {
        "temperatureBox": { "name": "Температура внутри шкафа", "unit": "°C", "color": "#1f77b4", "axis": "left" },
        "voltagePowerIn12b": { "name": "Входное напряжение 12В", "unit": "В", "color": "#ff7f0e", "axis": "left" },
        "voltageOut12b": { "name": "Выходное напряжение 12В", "unit": "В", "color": "#2ca02c", "axis": "left" },
        "voltageAkb": { "name": "Напряжение АКБ", "unit": "В", "color": "#d62728", "axis": "left" },
        "currentOut12b": { "name": "Выходной ток 12В", "unit": "А", "color": "#9467bd", "axis": "right" },
        "currentOut48b": { "name": "Выходной ток 48В", "unit": "А", "color": "#8c564b", "axis": "right" },
        "currentAkb": { "name": "Ток АКБ", "unit": "А", "color": "#e377c2", "axis": "right" },
        "wattHoursAkb": { "name": "Емкость АКБ", "unit": "Вт·ч", "color": "#7f7f7f", "axis": "right2" },
        // "visibleRange": { "name": "Метеорологическая видимость", "unit": "м", "color": "#bcbd22", "axis": "right2" },
        "sensor220b": { "name": "Наличие 220В", "unit": "", "color": "#17becf", "axis": "right2" },
        // "doorStatus": { "name": "Состояние двери", "unit": "", "color": "#aec7e8", "axis": "right2" },
        // "tdsH": { "name": "Высота от датчика", "unit": "м", "color": "#ffbb78", "axis": "right" },
        "tdsTds": { "name": "Минерализация (TDS)", "unit": "ppm", "color": "#98df8a", "axis": "right" },
        "tkosaT1": { "name": "Температура T1 (КОСА)", "unit": "°C", "color": "#ff9896", "axis": "left" },
        "tkosaT3": { "name": "Температура T3 (КОСА)", "unit": "°C", "color": "#c5b0d5", "axis": "left" }
    },
    "IWS": {
        "environmentTemperature": { "name": "Температура воздуха", "unit": "°C", "color": "#1f77b4", "axis": "left" },
        "humidityPercentage": { "name": "Влажность воздуха", "unit": "%", "color": "#ff7f0e", "axis": "right" },
        "dewPoint": { "name": "Точка росы", "unit": "°C", "color": "#2ca02c", "axis": "left" },
        "pressureHpa": { "name": "Давление (гПа)", "unit": "гПа", "color": "#d62728", "axis": "right2" },
        "pressureQNHHpa": { "name": "Давление QNH", "unit": "гПа", "color": "#8c564b", "axis": "right2" },
        "pressureMmHg": { "name": "Давление (мм рт. ст.)", "unit": "мм рт. ст.", "color": "#ffbb78", "axis": "right2" },
        "windSpeed": { "name": "Скорость ветра", "unit": "м/с", "color": "#9467bd", "axis": "right" },
        "windDirection": { "name": "Направление ветра", "unit": "°", "color": "#8c564b", "axis": "right2" },
        "windVSound": { "name": "Скорость звука", "unit": "м/с", "color": "#98df8a", "axis": "right" },
        "precipitationIntensity": { "name": "Интенсивность осадков", "unit": "мм/ч", "color": "#e377c2", "axis": "right" },
        "precipitationQuantity": { "name": "Количество осадков", "unit": "мм", "color": "#ff9896", "axis": "right" },
        "co2Level": { "name": "Уровень CO2", "unit": "ppm", "color": "#7f7f7f", "axis": "right2" }
        // ,"supplyVoltage": { "name": "Напряжение питания", "unit": "В", "color": "#bcbd22", "axis": "left" }
    }
};

/**
 * Основной класс для управления графиками
 */
class ChartManager {
    constructor(svgSelector, tooltipSelector, statsGridSelector) {
        console.log("Initializing ChartManager...");
        this.margin = { top: 60, right: 150, bottom: 70, left: 80 };
        this.width = 1200 - this.margin.left - this.margin.right;
        this.height = 600 - this.margin.top - this.margin.bottom;

        this.svg = d3.select(svgSelector)
            .append("g")
            .attr("transform", `translate(${this.margin.left},${this.margin.top})`);

        this.tooltip = d3.select(tooltipSelector);
        this.statsGrid = d3.select(statsGridSelector);
        this.showAllCheckbox = d3.select("#showAllMetrics");

        this.xScale = d3.scaleTime().range([0, this.width]);
        this.yLeft = d3.scaleLinear().range([this.height, 0]);
        this.yRight = d3.scaleLinear().range([this.height, 0]);
        this.yRight2 = d3.scaleLinear().range([this.height, 0]);

        this.initGroups();
        this.initEvents();

        this.currentGroup = null;
        this.currentSensorId = null;
        this.currentDays = 1;
        this.currentInterval = "hour";
        this.activeMetricKey = null;
        this.chartData = [];
    }

    initGroups() {
        this.xAxisGroup = this.svg.append("g").attr("transform", `translate(0, ${this.height})`);
        this.yLeftAxisGroup = this.svg.append("g").attr("class", "axis-left");
        this.yRightAxisGroup = this.svg.append("g").attr("class", "axis-right").attr("transform", `translate(${this.width}, 0)`);
        this.yRight2AxisGroup = this.svg.append("g").attr("class", "axis-right2").attr("transform", `translate(${this.width + 70}, 0)`);
        this.gridGroup = this.svg.append("g").attr("class", "grid");
        
        // Локализация D3 для русского языка
        this.ruLocale = d3.timeFormatDefaultLocale({
            dateTime: "%A, %e %B %Y г. %X",
            date: "%d.%m.%Y",
            time: "%H:%M:%S",
            periods: ["AM", "PM"],
            days: ["Воскресенье", "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота"],
            shortDays: ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
            months: ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"],
            shortMonths: ["Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек"]
        });

        this.bgRect = this.svg.append("rect")
            .attr("width", this.width).attr("height", this.height)
            .style("fill", "transparent")
            .style("pointer-events", "all");

        this.linesGroup = this.svg.append("g").attr("class", "lines").style("pointer-events", "none");
        this.pointsGroup = this.svg.append("g").attr("class", "points");

        this.verticalLine = this.svg.append("line")
            .attr("class", "vertical-line")
            .attr("y1", 0).attr("y2", this.height)
            .attr("x1", -10).attr("x2", -10)
            .style("opacity", 0);
    }

    initEvents() {
        const self = this;
        this.bgRect
            .on("mousemove", function(event) {
                if (!self.chartData || self.chartData.length === 0) return;
                const [x] = d3.pointer(event);
                const date = self.xScale.invert(x);
                const closest = self.chartData.reduce((prev, curr) => 
                    Math.abs(curr.date - date) < Math.abs(prev.date - date) ? curr : prev
                );
                self.verticalLine.attr("x1", self.xScale(closest.date)).attr("x2", self.xScale(closest.date)).style("opacity", 1);
                self.updateStatsPanel(closest);
                self.showTooltip(event, closest);
            })
            .on("mouseleave", () => {
                self.verticalLine.style("opacity", 0);
                if (self.chartData && self.chartData.length > 0) {
                    self.updateStatsPanel(self.chartData[self.chartData.length - 1]);
                }
                self.hideTooltip();
            });
    }

    async loadData(sensorId, group, days, interval) {
        console.log(`Loading data for sensor: ${sensorId}, group: ${group}, days: ${days}, interval: ${interval}`);
        this.currentSensorId = sensorId;
        this.currentGroup = group;
        this.currentDays = days;
        this.currentInterval = interval;

        let action = "Get" + group + "Data";
        if (interval === "hour") action += "Hour";
        else if (interval === "tenminutes") action += "TenMinuteInterval";

        try {
            const response = await $.get('/GraphsAndCharts/' + action, { sensorId, days });
            console.log("Data received:", response);
            if (response && response.measurements) {
                this.chartData = response.measurements.map(d => ({
                    ...d,
                    date: new Date(d.receivedAt)
                }));
                this.render();
            } else {
                console.warn("No measurements in response");
                this.chartData = [];
                this.render();
            }
        } catch (error) {
            console.error("Error loading chart data:", error);
        }
    }

    render() {
        console.log("Rendering chart...");
        const metrics = metricsGroups[this.currentGroup];
        if (!metrics) {
            console.error("No metrics config for group:", this.currentGroup);
            return;
        }
        
        const metricKeys = this.activeMetricKey ? [this.activeMetricKey] : Object.keys(metrics);

        if (this.chartData.length === 0) {
            console.warn("No data to render");
            // Clear chart
            this.linesGroup.selectAll("path").remove();
            this.pointsGroup.selectAll("g").remove();
            this.statsGrid.html("<div class='no-data'>Нет данных за выбранный период</div>");
            return;
        }

        this.xScale.domain(d3.extent(this.chartData, d => d.date));

        const getDomain = (axis) => {
            const keys = Object.keys(metrics).filter(k => metrics[k].axis === axis);
            if (keys.length === 0) return [0, 100];
            
            let allValues = this.chartData.flatMap(d => keys.map(k => d[k])).filter(v => v !== null && v !== undefined);
            if (allValues.length === 0) return [0, 100];
            
            const min = d3.min(allValues);
            const max = d3.max(allValues);
            return [min * 0.9, max * 1.1];
        };

        this.yLeft.domain(getDomain("left"));
        this.yRight.domain(getDomain("right"));
        this.yRight2.domain(getDomain("right2"));

        this.yLeftAxisGroup.transition().call(d3.axisLeft(this.yLeft).ticks(8));
        this.yRightAxisGroup.transition().call(d3.axisRight(this.yRight).ticks(6));
        this.yRight2AxisGroup.transition().call(d3.axisRight(this.yRight2).ticks(6));
        
        // Настройка формата оси X с учетом локализации
        const multiFormat = (date) => {
            return (d3.timeSecond(date) < date ? d3.timeFormat(".%L")
                : d3.timeMinute(date) < date ? d3.timeFormat("%H:%M")
                : d3.timeHour(date) < date ? d3.timeFormat("%H:%M")
                : d3.timeDay(date) < date ? d3.timeFormat("%H:%M")
                : d3.timeMonth(date) < date ? (d3.timeWeek(date) < date ? d3.timeFormat("%a %d") : d3.timeFormat("%b %d"))
                : d3.timeYear(date) < date ? d3.timeFormat("%B")
                : d3.timeFormat("%Y"))(date);
        };

        this.xAxisGroup.transition().call(d3.axisBottom(this.xScale)
            .ticks(10)
            .tickFormat(multiFormat));

        this.gridGroup.call(d3.axisLeft(this.yLeft).ticks(6).tickSize(-this.width).tickFormat(""));

        this.linesGroup.selectAll("path").remove();
        metricKeys.forEach(key => {
            const m = metrics[key];
            const scale = m.axis === 'left' ? this.yLeft : (m.axis === 'right' ? this.yRight : this.yRight2);
            const lineData = this.chartData.filter(d => d[key] !== null && d[key] !== undefined);
            
            this.linesGroup.append("path")
                .datum(lineData)
                .attr("fill", "none")
                .attr("stroke", m.color)
                .attr("stroke-width", 2)
                .attr("opacity", 0.7)
                .attr("d", d3.line()
                    .x(d => this.xScale(d.date))
                    .y(d => scale(d[key]))
                    .curve(d3.curveMonotoneX)
                );
        });

        this.pointsGroup.selectAll("g").remove();
        metricKeys.forEach(key => {
            const m = metrics[key];
            const scale = m.axis === 'left' ? this.yLeft : (m.axis === 'right' ? this.yRight : this.yRight2);
            const g = this.pointsGroup.append("g");
            const pointData = this.chartData.filter(d => d[key] !== null && d[key] !== undefined);

            g.selectAll("circle")
                .data(pointData)
                .enter()
                .append("circle")
                .attr("cx", d => this.xScale(d.date))
                .attr("cy", d => scale(d[key]))
                .attr("r", 3)
                .attr("fill", m.color)
                .attr("stroke", "white")
                .attr("stroke-width", 0.5);
        });

        this.updateStatsPanel(this.chartData[this.chartData.length - 1]);
    }

    updateStatsPanel(pointData) {
        if (!pointData) return;
        const metrics = metricsGroups[this.currentGroup];
        if (!metrics) return;
        
        this.statsGrid.html("");
        Object.keys(metrics).forEach(key => {
            const m = metrics[key];
            const val = pointData[key];
            const card = this.statsGrid.append("div")
                .attr("class", "stat-card")
                .classed("active", key === this.activeMetricKey)
                .on("click", () => {
                    this.activeMetricKey = (this.activeMetricKey === key) ? null : key;
                    this.render();
                });

            const labelWrapper = card.append("div").attr("class", "label-content");
            labelWrapper.append("span").attr("class", "stat-dot").style("background", m.color);
            labelWrapper.append("span").attr("class", "stat-label").text(m.name);
            
            const displayVal = (val !== null && val !== undefined) ? `${val.toFixed(2)} ${m.unit}` : "—";
            card.append("span").attr("class", "stat-value").text(displayVal);
        });
    }

    showTooltip(event, d) {
        const metrics = metricsGroups[this.currentGroup];
        const showAll = this.showAllCheckbox.property("checked");        this.tooltip.style("display", "block");
        
        let html = `<div class="tooltip-title">${d.date.toLocaleString()}</div>`;
        
        if (showAll) {
            Object.keys(metrics).forEach(key => {
                const m = metrics[key];
                const val = d[key];
                const displayVal = (val !== null && val !== undefined) ? `${val.toFixed(2)} ${m.unit}` : "—";
                html += `<div class="tooltip-row">
                    <span style="color: ${m.color}">●</span>
                    <span>${m.name}</span>
                    <span class="tooltip-value">${displayVal}</span>
                </div>`;
            });
        } else {
            const [mouseX, mouseY] = d3.pointer(event, this.svg.node());
            let closestKey = null;
            let minDist = Infinity;

            Object.keys(metrics).forEach(key => {
                const m = metrics[key];
                const val = d[key];
                if (val === null || val === undefined) return;
                
                const scale = m.axis === 'left' ? this.yLeft : (m.axis === 'right' ? this.yRight : this.yRight2);
                const dist = Math.abs(scale(val) - mouseY);
                if (dist < minDist) {
                    minDist = dist;
                    closestKey = key;
                }
            });

            if (closestKey) {
                const m = metrics[closestKey];
                const val = d[closestKey];
                html += `<div class="tooltip-row">
                    <span style="color: ${m.color}">●</span>
                    <span>${m.name}</span>
                    <span class="tooltip-value">${val.toFixed(2)} ${m.unit}</span>
                </div>`;
            }
        }
        this.tooltip.html(html)
            .style("left", (event.clientX + 15) + "px")
            .style("top", (event.clientY + 15) + "px");
    }

    hideTooltip() {
        this.tooltip.style("display", "none");
    }

    resetFilter() {
        this.activeMetricKey = null;
        this.render();
    }
}
