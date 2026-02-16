using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BurstroyMonitoring.Data.Models.ViewModels
{
    public class DataFilterViewModel
    {
        [Display(Name = "Начальная дата")]
        public DateTime? StartDate { get; set; }
        
        [Display(Name = "Конечная дата")]
        public DateTime? EndDate { get; set; }
        
        [Display(Name = "Активный датчик")]
        public bool? SensorIsActive { get; set; }
        
        [Display(Name = "Активный пост")]
        public bool? PostIsActive { get; set; }
        
        [Display(Name = "Серийный номер")]
        public string? SerialNumber { get; set; }
        
        [Display(Name = "Конечная точка")]
        public string? EndpointName { get; set; }
        
        [Display(Name = "Выводить по")]
        public string? PageSize { get; set; } = "10";
        
        [Display(Name = "Выбранные поля")]
        [JsonInclude]
        public List<string> SelectedFields { get; set; } = new List<string>();
        
        public DataFilterViewModel() { }
    }
}