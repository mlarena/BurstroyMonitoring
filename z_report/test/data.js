// --- МЕТРИКИ ПО ГРУППАМ ---
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
    "roadAngle": { "name": "Угол наклона дороги", "unit": "°", "color": "#98df8a", "axis": "right2" },
    "freezeTemperature": { "name": "Температура замерзания", "unit": "°C", "color": "#ff9896", "axis": "left" },
    "distanceToSurface": { "name": "Расстояние до поверхности", "unit": "мм", "color": "#c5b0d5", "axis": "right" }
  },
  "DUST": {
    "pm10Act": { "name": "Концентрация PM10", "unit": "мкг/м³", "color": "#1f77b4", "axis": "left" },
    "pm25Act": { "name": "Концентрация PM2.5", "unit": "мкг/м³", "color": "#ff7f0e", "axis": "left" },
    "pm1Act": { "name": "Концентрация PM1", "unit": "мкг/м³", "color": "#2ca02c", "axis": "left" },
    "pm10Awg": { "name": "Концентрация PM10 средняя", "unit": "мкг/м³", "color": "#d62728", "axis": "left" },
    "pm25Awg": { "name": "Концентрация PM2.5 средняя", "unit": "мкг/м³", "color": "#9467bd", "axis": "left" },
    "pm1Awg": { "name": "Концентрация PM1 средняя", "unit": "мкг/м³", "color": "#8c564b", "axis": "left" },
    "flowProbe": { "name": "Расход воздуха", "unit": "л/мин", "color": "#e377c2", "axis": "right" },
    "temperatureProbe": { "name": "Температура пробоотборника", "unit": "°C", "color": "#7f7f7f", "axis": "right2" },
    "humidityProbe": { "name": "Влажность пробоотборника", "unit": "%", "color": "#bcbd22", "axis": "right" }
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
    "visibleRange": { "name": "Метеорологическая видимость", "unit": "м", "color": "#bcbd22", "axis": "right2" },
    "sensor220b": { "name": "Наличие 220В", "unit": "", "color": "#17becf", "axis": "right2" },
    "doorStatus": { "name": "Состояние двери", "unit": "", "color": "#aec7e8", "axis": "right2" },
    "tdsH": { "name": "Высота от датчика", "unit": "м", "color": "#ffbb78", "axis": "right" },
    "tdsTds": { "name": "Минерализация (TDS)", "unit": "ppm", "color": "#98df8a", "axis": "right" },
    "tkosaT1": { "name": "Температура T1 (КОСА)", "unit": "°C", "color": "#ff9896", "axis": "left" },
    "tkosaT3": { "name": "Температура T3 (КОСА)", "unit": "°C", "color": "#c5b0d5", "axis": "left" }
  },
  "IWS": {
    "environmentTemperature": { "name": "Температура воздуха", "unit": "°C", "color": "#1f77b4", "axis": "left" },
    "humidityPercentage": { "name": "Влажность воздуха", "unit": "%", "color": "#ff7f0e", "axis": "right" },
    "dewPoint": { "name": "Точка росы", "unit": "°C", "color": "#2ca02c", "axis": "left" },
    "pressureHpa": { "name": "Давление (гПа)", "unit": "гПа", "color": "#d62728", "axis": "right2" },
    "windSpeed": { "name": "Скорость ветра", "unit": "м/с", "color": "#9467bd", "axis": "right" },
    "windDirection": { "name": "Направление ветра", "unit": "°", "color": "#8c564b", "axis": "right2" },
    "precipitationIntensity": { "name": "Интенсивность осадков", "unit": "мм/ч", "color": "#e377c2", "axis": "right" },
    "co2Level": { "name": "Уровень CO2", "unit": "ppm", "color": "#7f7f7f", "axis": "right2" },
    "supplyVoltage": { "name": "Напряжение питания", "unit": "В", "color": "#bcbd22", "axis": "left" }
  }
};

// --- ГЕНЕРАЦИЯ ДАННЫХ (имитация API) ---
function fetchChartData(group, days, interval) {
    const numPoints = interval === 'raw' ? 200 : (interval === 'tenminutes' ? 144 : 72);
    const now = new Date();
    const measurements = [];

    function generateValue(base, amp, noise = 0.5, i) {
        const val = base + amp * Math.sin((i / numPoints) * 2 * Math.PI) + (Math.random() - 0.5) * noise;
        return Number(val.toFixed(2));
    }

    for (let i = 0; i < numPoints; i++) {
        const date = new Date(now.getTime() - (numPoints - i) * (days * 24 * 60 * 60 * 1000 / numPoints));
        const measurement = {
            receivedAt: date.toISOString()
        };

        // Заполняем данными для всех метрик группы
        Object.keys(metricsGroups[group]).forEach(metricKey => {
            measurement[metricKey] = generateValue(Math.random() * 50, Math.random() * 20, 2, i);
        });

        measurements.push(measurement);
    }

    return {
        sensorId: 1,
        serialNumber: "SN-TEST-123",
        endpointName: `${group.toLowerCase()}_local_Москва`,
        postName: "Москва",
        measurements: measurements
    };
}
