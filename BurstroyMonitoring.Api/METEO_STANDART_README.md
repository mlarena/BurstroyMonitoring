# Документация API: Получение стандартных метеоданных

Данный раздел описывает работу с методом получения метеорологических данных в стандаре ГОСТ Р 71094—2024.

## Эндпоинт

**URL:** `/api/v1/monitoring-posts/{monitoringPostId}/meteo-standart`  
**Метод:** `GET`  
**Описание:** Возвращает последние актуальные метеоданные для указанного поста мониторинга, агрегированные из различных датчиков (метеостанция, датчики дорожного покрытия, датчики видимости).

### Параметры запроса

| Параметр | Тип | Обязательный | Описание |
| :--- | :--- | :--- | :--- |
| `monitoringPostId` | `int` | Да | Уникальный идентификатор поста мониторинга. |

## Структура ответа от АДМС

Ответ представляет собой JSON-объект со следующими полями:

| Поле | Тип | Описание | Источник (Тип датчика) |
| :--- | :--- | :--- | :--- |
| `meteo_t_air` | `float` | Температура воздуха, °C | IWS |
| `meteo_humidity` | `float` | Относительная влажность воздуха, % | IWS |
| `meteo_air_pressure` | `float` | Атмосферное давление, гПа | IWS |
| `meteo_wind_velocity` | `float` | Скорость ветра, м/с | IWS |
| `meteo_wind_gusts` | `float` | Порывы ветра, м/с | IWS |
| `meteo_wind_direction` | `float` | Направление ветра, град | IWS |
| `meteo_precip_amount` | `float` | Количество осадков, мм | IWS |
| `meteo_precip_intensity` | `float` | Интенсивность осадков, мм/ч | IWS |
| `meteo_view_distance` | `int` | Метеорологическая дальность видимости, м | DOV |
| `meteo_t_road` | `float` | Температура поверхности дорожного покрытия, °C | DSPD |
| `meteo_t_underroad` | `float` | Температура дорожной одежды, °C | DSPD |
| `meteo_t_base` | `float` | Температура грунта земляного полотна, °C | - |
| `meteo_condition_road` | `int` | Код состояния поверхности дороги (см. ниже) | DSPD |
| `meteo_volhumidity_base` | `float` | Объемная влажность дорожной одежды, % | - |
| `meteo_layer_water` | `float` | Высота слоя воды на поверхности, мм | DSPD |
| `meteo_sit_intensity` | `int` | Наличие осадков (0 — нет, 1 — да) | Расчетное (IWS) |
| `meteo_dew_point` | `float` | Температура точки росы, °C | IWS |
| `meteo_layer_snow` | `float` | Высота слоя снега на поверхности, мм | DSPD |
| `meteo_layer_ice` | `float` | Высота слоя льда на поверхности, мм | DSPD |
| `meteo_precip_code` | `int` | Код осадков (см. ниже) | IWS |

### Особенности данных

1.  **Значение `-9999`**: Если в ответе поле содержит значение `-9999`, это означает, что данные для этого параметра в данный момент недоступны или источник данных не определен.
2.  **Коды состояния дороги (`meteo_condition_road`)**:
    *   `1` — сухо;
    *   `2` — мокро (вода);
    *   `3` — лед;
    *   `4` — реагент;
    *   `5` — реагент со льдом.
3.  **Коды осадков (`meteo_precip_code`)**:
    *   `1` — дождь;
    *   `2` — дождь со снегом;
    *   `3` — снег.

## Пример запроса

```http
GET /api/v1/monitoring-posts/10/meteo-standart
```

## Пример ответа

```json
{
  "meteo_t_air": 15.5,
  "meteo_humidity": 65.0,
  "meteo_air_pressure": 1013.2,
  "meteo_wind_velocity": 3.4,
  "meteo_wind_gusts": 3.4,
  "meteo_wind_direction": 180.0,
  "meteo_precip_amount": 0.0,
  "meteo_precip_intensity": 0.0,
  "meteo_view_distance": 10000,
  "meteo_t_road": 18.2,
  "meteo_t_underroad": -9999,
  "meteo_t_base": -9999,
  "meteo_condition_road": 1,
  "meteo_volhumidity_base": -9999,
  "meteo_layer_water": 0.0,
  "meteo_sit_intensity": 0,
  "meteo_dew_point": 8.5,
  "meteo_layer_snow": 0.0,
  "meteo_layer_ice": 0.0,
  "meteo_precip_code": 0
}
```

## Ошибки

*   `404 Not Found`: Пост мониторинга не найден.
*   `500 Internal Server Error`: Ошибка при получении данных из базы данных.
