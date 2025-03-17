using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ConnectionLogger.AsyncReceiver.Interfaces;
using Microsoft.Extensions.Options;
using ConnectionLogger.AsyncReceiver.Configurations;

namespace ConnectionLogger.AsyncReceiver.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<ApiService> _logger;
    private readonly AppSettings _appSettings;

    public ApiService(HttpClient httpClient, IOptions<AppSettings> appSettings, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _appSettings = appSettings.Value;
        _baseUrl = _appSettings.ApiSettings.BaseUrl;
        _logger = logger;
    }

    public async Task<string> GetDataFromServer(string pathUrl)
    {
        string url = $"{_baseUrl}/{pathUrl}";
        _logger.LogInformation("Отправка запроса к {Url}", url);

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Ответ сервера: {Response}", content);

        return content;
    }
}
