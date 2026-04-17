namespace BurstroyMonitoring.TCM.Helpers
{
    public static class RoadStatusHelper
    {
        public static string GetRoadStatusText(int? code)
        {
            return code switch
            {
                0 => "Измерение невозможно",
                1 => "Сухо",
                2 => "Влажно",
                3 => "Мокро",
                5 => "Лёд",
                6 => "Снег",
                9 => "Противогололёдные материалы",
                10 => "Снег со льдом",
                11 => "Сухо, небольшие следы снега/льда",
                49 => "Снег, посыпанный песком",
                _ => code?.ToString() ?? "—"
            };
        }
    }
}
