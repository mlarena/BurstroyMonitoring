using Microsoft.AspNetCore.Mvc;
using BurstroyMonitoring.TCM.Services;
using System.Globalization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace BurstroyMonitoring.TCM.Controllers;

public class ForecastController : Controller
{
    private readonly ILogger<ForecastController> _logger;
    private readonly GismeteoService _gismeteoService;
    private readonly string _baseUrl = "https://api.gismeteo.net/v3/weather";

    public ForecastController(ILogger<ForecastController> logger, GismeteoService gismeteoService)
    {
        _logger = logger;
        _gismeteoService = gismeteoService;
    }

    public IActionResult Index()
    {
        ViewData["Message"] = "Прогноз погоды";
        return View();
    }

    public async Task<IActionResult> GetCurrentWeather(double latitude = 55.7558, double longitude = 37.6173)
    {
        _logger.LogInformation("Received request for GetCurrentWeather with latitude: {Latitude}, longitude: {Longitude}", latitude, longitude);
        string url = $"{_baseUrl}/current/?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}";
        return await ProxyRequest(url, latitude, longitude);
    }

    public async Task<IActionResult> GetForecastH1(double latitude = 55.7558, double longitude = 37.6173)
    {
        _logger.LogInformation("Received request for GetForecastH1 with latitude: {Latitude}, longitude: {Longitude}", latitude, longitude);
        string url = $"{_baseUrl}/forecast/h1/?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}";
        return await ProxyRequest(url, latitude, longitude);
    }

    public async Task<IActionResult> GetForecastH3(double latitude = 55.7558, double longitude = 37.6173)
    {
        _logger.LogInformation("Received request for GetForecastH3 with latitude: {Latitude}, longitude: {Longitude}", latitude, longitude);
        string url = $"{_baseUrl}/forecast/h3/?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}";
        return await ProxyRequest(url, latitude, longitude);
    }

    public async Task<IActionResult> GetForecastH6(double latitude = 55.7558, double longitude = 37.6173)
    {
        _logger.LogInformation("Received request for GetForecastH6 with latitude: {Latitude}, longitude: {Longitude}", latitude, longitude);
        string url = $"{_baseUrl}/forecast/h6/?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}";
        return await ProxyRequest(url, latitude, longitude);
    }

    public async Task<IActionResult> GetForecastH24(double latitude = 55.7558, double longitude = 37.6173)
    {
        _logger.LogInformation("Received request for GetForecastH24 with latitude: {Latitude}, longitude: {Longitude}", latitude, longitude);
        string url = $"{_baseUrl}/forecast/h24/?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}";
        return await ProxyRequest(url, latitude, longitude);
    }

    private async Task<IActionResult> ProxyRequest(string url, double latitude, double longitude)
    {
        try
        {
            var jsonResponse = await _gismeteoService.GetAsyncHTTPS_GISMETEP(url);
            return Content(jsonResponse, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data from {Url}", url);
            return StatusCode(500, new { error = "Error fetching weather data" });
        }
    }
}
