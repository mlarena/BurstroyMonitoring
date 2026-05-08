using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace BurstroyMonitoring.TCM.Services;

public class GismeteoService
{
    private readonly string _gismeteoToken;

    public GismeteoService(IConfiguration configuration)
    {
        // Используем токен из файла или конфигурации
        _gismeteoToken = "eb439e20-c234-4c21-bd4d-932584f54932";
    }

    public async Task<string> GetAsyncHTTPS_GISMETEP(string url)
    {
        using var clientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };
        
        using var client = new HttpClient(clientHandler);
        client.DefaultRequestHeaders.Add("User-Agent", "C# App");
        client.DefaultRequestHeaders.Add("X-Gismeteo-Token", _gismeteoToken);

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
