using System.ComponentModel.DataAnnotations;

namespace BurstroyMonitoring.Data.Models
{
    public class WorkerConfigEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Ключ")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Значение обязательно")]
        [Display(Name = "Значение")]
        public string Value { get; set; } = string.Empty;

        [Display(Name = "Тип данных")]
        public string DataType { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Последнее изменение")]
        public DateTime LastModified { get; set; }

        [Display(Name = "Изменено")]
        public string? ModifiedBy { get; set; }

        [Display(Name = "Активно")]
        public bool IsActive { get; set; }

        public bool IsBoolean => DataType == "boolean";
        public bool IsInteger => DataType == "integer";
        public bool IsDecimal => DataType == "decimal";
        public bool IsJson => DataType == "json";
    }
}