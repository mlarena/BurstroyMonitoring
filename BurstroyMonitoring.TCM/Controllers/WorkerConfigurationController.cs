using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BurstroyMonitoring.Data;
using BurstroyMonitoring.Data.Models;
using System.Diagnostics;

namespace BurstroyMonitoring.TCM.Controllers
{
    public class WorkerConfigurationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkerConfigurationController> _logger;

        public WorkerConfigurationController(
            ApplicationDbContext context,
            ILogger<WorkerConfigurationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: WorkerConfiguration
        public async Task<IActionResult> Index()
        {
            var configurations = await _context.WorkerConfigurations
                .Where(w => w.IsActive)
                .OrderBy(w => w.Key)
                .Select(w => new WorkerConfigEditViewModel
                {
                    Id = w.Id,
                    Key = w.Key,
                    Value = w.Value,
                    DataType = w.DataType,
                    Description = w.Description,
                    LastModified = w.LastModified.ToLocalTime(),
                    ModifiedBy = w.ModifiedBy,
                    IsActive = w.IsActive
                })
                .ToListAsync();

            return View(configurations);
        }

        // POST: WorkerConfiguration/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string value)
        {
            _logger.LogInformation("WorkerConfiguration Edit called for id: {Id} with value: {Value}", id, value);
            
            var config = await _context.WorkerConfigurations.FindAsync(id);
            
            if (config == null)
            {
                TempData["ErrorMessage"] = "Конфигурация не найдена";
                return RedirectToAction(nameof(Index));
            }

            // Простая валидация по типу данных
            if (!IsValidValue(config.DataType, value))
            {
                TempData["ErrorMessage"] = $"Некорректное значение для типа {config.DataType}";
                return RedirectToAction(nameof(Index));
            }

            config.Value = value;
            config.LastModified = DateTime.UtcNow;
            config.ModifiedBy = User.Identity?.Name ?? "System";

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Configuration updated successfully: {Key} = {Value}", config.Key, value);
                TempData["SuccessMessage"] = "Значение обновлено";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration");
                TempData["ErrorMessage"] = "Ошибка при обновлении значения";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsValidValue(string dataType, string value)
        {
            return dataType.ToLower() switch
            {
                "boolean" => bool.TryParse(value, out _),
                "integer" => int.TryParse(value, out _),
                "decimal" => decimal.TryParse(value, out _),
                "json" => IsValidJson(value),
                "string" => true,
                _ => false
            };
        }

        private bool IsValidJson(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            try
            {
                JsonDocument.Parse(value);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}