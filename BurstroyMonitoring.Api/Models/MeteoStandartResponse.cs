using System;

namespace BurstroyMonitoring.Api.Models
{
    public class MeteoStandartResponse
    {
        /// <summary>
        /// Температура воздуха, °C
        /// </summary>
        public float? MeteoT_Air { get; set; }

        /// <summary>
        /// Относительная влажность воздуха, %
        /// </summary>
        public float? MeteoHumidity { get; set; }

        /// <summary>
        /// Атмосферное давление, гПа
        /// </summary>
        public float? MeteoAir_Pressure { get; set; }

        /// <summary>
        /// Скорость ветра, м/с
        /// </summary>
        public float? MeteoWind_Velocity { get; set; }

        /// <summary>
        /// Порывы ветра, м/с (пустой)
        /// </summary>
        public float? MeteoWind_Gusts { get; set; }

        /// <summary>
        /// Направление ветра, град
        /// </summary>
        public float? MeteoWind_Direction { get; set; }

        /// <summary>
        /// Количество осадков, мм
        /// </summary>
        public float? MeteoPrecip_Amount { get; set; }

        /// <summary>
        /// Интенсивность осадков, мм/ч
        /// </summary>
        public float? MeteoPrecip_Intensity { get; set; }

        /// <summary>
        /// Метеорологическая дальность видимости, м (пустой)
        /// </summary>
        public int? MeteoView_Distance { get; set; }

        /// <summary>
        /// Температура поверхности дорожного покрытия, °C (пустой)
        /// </summary>
        public float? MeteoT_Road { get; set; }

        /// <summary>
        /// Температура дорожной одежды, °C (пустой)
        /// </summary>
        public float? MeteoT_Underroad { get; set; }

        /// <summary>
        /// Температура грунта земляного полотна, °C (пустой)
        /// </summary>
        public float? MeteoT_Base { get; set; }

        /// <summary>
        /// Код состояния поверхности дороги (пустой)
        /// </summary>
        public int? MeteoCondition_Road { get; set; }

        /// <summary>
        /// Объемная влажность дорожной одежды, % (пустой)
        /// </summary>
        public float? MeteoVolhumidity_Base { get; set; }

        /// <summary>
        /// Высота слоя воды на поверхности, мм (пустой)
        /// </summary>
        public float? MeteoLayer_Water { get; set; }

        /// <summary>
        /// Наличие осадков (PrecipitationType)
        /// </summary>
        public int? MeteoSit_Intensity { get; set; }

        /// <summary>
        /// Температура точки росы, °C
        /// </summary>
        public float? MeteoDew_Point { get; set; }

        /// <summary>
        /// Высота слоя снега на поверхности (пустой)
        /// </summary>
        public float? MeteoLayer_Snow { get; set; }

        /// <summary>
        /// Высота слоя льда на поверхности, мм (пустой)
        /// </summary>
        public float? MeteoLayer_Ice { get; set; }

        /// <summary>
        /// Код осадков (PrecipitationType)
        /// </summary>
        public int? MeteoPrecip_Code { get; set; }
    }
}
