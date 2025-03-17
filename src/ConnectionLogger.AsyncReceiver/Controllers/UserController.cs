using Microsoft.AspNetCore.Mvc;
using ConnectionLogger.Messaging.Messages;
using System.Net;
using System.Net.Sockets;
using ConnectionLogger.AsyncReceiver;
using ConnectionLogger.AsyncReceiver.Interfaces;
using ConnectionLogger.AsyncReceiver.Controllers.Models;
using System.Text.RegularExpressions;
using ConnectionLogger.AsyncReceiver.Services;
using System;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IRequestProduser _requestProduser;
    private readonly IApiService _apiService;

    public UserController(IRequestProduser requestProduser, ILogger<UserController> logger,  IApiService userService)
    {
        _requestProduser = requestProduser;
        _logger = logger;
        _apiService = userService;
    }

    /// <summary>
    /// Через RabbitMQ
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("{userId}/connect")]
    public async Task<IActionResult> ConnectUser(long userId, [FromBody] UserConnection request)
    {
        if (request == null || !IsValidIp(request.Ip))
        {
            return BadRequest(new { message = "Invalid IP address" });
        }

        string ip = request.Ip;

        string protocol = GetIpProtocol(ip);

        var message = new ConnectUserMessage
        {
            UserId = userId,
            IpAddress = ip,
            Protocol = protocol
        };

        var type = typeof(ConnectUserMessage).Name;
        var headers = new Dictionary<string, object>
        {
            { "type",  type}
        };

        var correlationId = Guid.NewGuid().ToString();
        await _requestProduser.SendAsync(message, correlationId, headers);

        return Accepted(new { message = "Message accepted by RabbitMQ", correlationId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetData(int id)
    {
        _logger.LogInformation("Запрос клиента для ID {Id}", id);
        var pathUrl = $"api/users/{id}";
        var result = await _apiService.GetDataFromServer(pathUrl);

        return Ok(new { Response = result });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsersByIpPart([FromQuery] string ipPart, [FromQuery] string protocol)
    {
        _logger.LogInformation("User search request by IP: ipPart={IpPart}, protocol={Protocol}", ipPart, protocol);

        if (string.IsNullOrWhiteSpace(ipPart))
        {
            _logger.LogWarning("User search rejected: ipPart is empty");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid IP",
                Detail = "ipPart must not be empty",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!IsValidIpProtocol(protocol))
        {
            _logger.LogWarning("User search rejected: invalid protocol {Protocol}", protocol);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid protocol",
                Detail = "Protocol must be 'IPv4' or 'IPv6'",
                Status = StatusCodes.Status400BadRequest
            });
        }

        protocol = NormalizeIpProtocol(protocol);

        if (!IsValidIpStart(ipPart, protocol))
        {
            _logger.LogWarning("User search rejected: {IpPart} does not match protocol {Protocol}", ipPart, protocol);
            return BadRequest(new ProblemDetails
            {
                Title = "IP and protocol mismatch",
                Detail = "ipPart does not match the specified protocol type",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var result = await _apiService.GetDataFromServer($"api/users/search?ipPart={ipPart}&protocol={protocol}");

            _logger.LogInformation("Search completed, found {ResultCount} records", result.Length);
            return Ok(new { Response = result });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error while making request to the user search API");
            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Title = "Service error",
                Detail = "Failed to retrieve data from the user service",
                Status = StatusCodes.Status502BadGateway
            });
        }
    }



    [HttpGet("{userId}/ips")]
    public async Task<IActionResult> GetUserIps(long userId)
    {
        var result = await _apiService.GetDataFromServer($"api/users/{userId}/ips");

        _logger.LogInformation("Search completed, found {ResultCount} records", result.Length);
        return Ok(new { Response = result });
    }

    private bool IsValidIp(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out _);
    }

    private string GetIpProtocol(string ipAddress)
    {
        if (IPAddress.TryParse(ipAddress, out IPAddress ip))
        {
            return ip.AddressFamily == AddressFamily.InterNetwork ? "IPv4" :
                   ip.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" :
                   "Unknown";
        }

        return "Invalid";
    }

    private bool IsValidIpProtocol(string protocol)
    {
        return Regex.IsMatch(protocol, @"^ipv[46]$", RegexOptions.IgnoreCase);
    }

    private string? NormalizeIpProtocol(string protocol)
    {
        string lower = protocol.ToLower().Trim();
        return lower switch
        {
            "ipv4" => "IPv4",
            "ipv6" => "IPv6",
            _ => ""
        };
    }

    private bool IsValidIpStart(string segment, string protocol)
    {
        if (protocol.Equals("IPv4", StringComparison.OrdinalIgnoreCase))
        {
            return IsValidIPv4Start(segment);
        }
        else if (protocol.Equals("IPv6", StringComparison.OrdinalIgnoreCase))
        {
            return IsValidIPv6Start(segment);
        }

        return false;
    }

    private bool IsValidIPv4Start(string segment)
    {
        // Проверка: сегмент должен быть началом IPv4 (X, X.X, X.X.X)
        if (Regex.IsMatch(segment, @"^\d{1,3}(\.\d{1,3}){0,2}$"))
        {
            string[] parts = segment.Split('.');
            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    return false;
            }

            return true;
        }

        return false;
    }

    private bool IsValidIPv6Start(string segment)
    {
        // Поддержка "::" (сокращённого IPv6)
        if (segment == "::" || segment.StartsWith("::"))
        {
            return true;
        }

        // IPv6-группы: 1-4 шестнадцатеричных символа (частично введённые тоже допустимы)
        if (Regex.IsMatch(segment, @"^[0-9a-fA-F]{1,4}$"))
        {
            return true;
        }

        // Частично введённые группы, допускающие двоеточие в конце (2001:, fe80::, 20:)
        if (Regex.IsMatch(segment, @"^([0-9a-fA-F]{1,4}:)+[0-9a-fA-F]{0,4}$"))
        {
            return true;
        }

        return false;
    }
}
