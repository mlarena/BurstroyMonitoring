using System;

namespace BurstroyMonitoring.Api.Models
{
    public class MeteoStandartResponse
    {
        /// <summary>
        /// Температура воздуха, °C
        /// </summary>
        public float? meteo_t_air { get; set; }

        /// <summary>
        /// Относительная влажность воздуха, %
        /// </summary>
        public float? meteo_humidity { get; set; }

        /// <summary>
        /// Атмосферное давление, гПа
        /// </summary>
        public float? meteo_air_pressure { get; set; }

        /// <summary>
        /// Скорость ветра, м/с
        /// </summary>
        public float? meteo_wind_velocity { get; set; }

        /// <summary>
        /// Порывы ветра, м/с (пустой)
        /// </summary>
        public float? meteo_wind_gusts { get; set; }

        /// <summary>
        /// Направление ветра, град
        /// </summary>
        public float? meteo_wind_direction { get; set; }

        /// <summary>
        /// Количество осадков, мм
        /// </summary>
        public float? meteo_precip_amount { get; set; }

        /// <summary>
        /// Интенсивность осадков, мм/ч
        /// </summary>
        public float? meteo_precip_intensity { get; set; }

        /// <summary>
        /// Метеорологическая дальность видимости, м (пустой)
        /// </summary>
        public int? meteo_view_distance { get; set; }

        /// <summary>
        /// Температура поверхности дорожного покрытия, °C (пустой)
        /// </summary>
        public float? meteo_t_road { get; set; }

        /// <summary>
        /// Температура дорожной одежды, °C (пустой)
        /// </summary>
        public float? meteo_t_underroad  { get; set; }

        /// <summary>
        /// Температура грунта земляного полотна, °C (пустой)
        /// </summary>
        public float? meteo_t_base { get; set; }

        /// <summary>
        /// Код состояния поверхности дороги (пустой)
        /// </summary>
        public int? meteo_condition_road { get; set; }

        /// <summary>
        /// Объемная влажность дорожной одежды, % (пустой)
        /// </summary>
        public float? meteo_volhumidity_base  { get; set; }

        /// <summary>
        /// Высота слоя воды на поверхности, мм (пустой)
        /// </summary>
        public float? meteo_layer_water { get; set; }

        /// <summary>
        /// Наличие осадков (PrecipitationType)
        /// </summary>
        public int? meteo_sit_intensity { get; set; }

        /// <summary>
        /// Температура точки росы, °C
        /// </summary>
        public float? meteo_dew_point { get; set; }

        /// <summary>
        /// Высота слоя снега на поверхности (пустой)
        /// </summary>
        public float? meteo_layer_snow { get; set; }

        /// <summary>
        /// Высота слоя льда на поверхности, мм (пустой)
        /// </summary>
        public float? meteo_layer_ice { get; set; }

        /// <summary>
        /// Код осадков (PrecipitationType)
        /// </summary>
        public int? meteo_precip_code  { get; set; }
    }
}
