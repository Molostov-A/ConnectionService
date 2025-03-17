using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ConnectionLogger.AsyncReceiver.Interfaces;

namespace ConnectionLogger.AsyncReceiver.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly ILogger<UserService> _logger;

    public UserService(HttpClient httpClient, IConfiguration configuration, ILogger<UserService> logger)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ApiSettings:BaseUrl"];
        _logger = logger;
    }

    public async Task<string> GetDataFromServer(int id)
    {
        string url = $"{_baseUrl}/{id}";
        _logger.LogInformation("Отправка запроса к {Url}", url);

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Ответ сервера: {Response}", content);

        return content;
    }
}
