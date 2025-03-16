using Microsoft.AspNetCore.Mvc;
using ModelsLibrary.Models;
using System.Net;
using System.Net.Sockets;
using WebProducer;
using WebProducer.Interfaces;
using WebProducer.Controllers.Models;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IRequestProduser _requestProduser;
    private readonly ResponsePool _responsePool;

    public UserController(IRequestProduser requestProduser, ResponsePool responsePool)
    {
        _requestProduser = requestProduser;
        _responsePool = responsePool;
    }

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

        // Ожидание ответа
        ResponseResult response = null;
        while (response == null)
        {
            response = _responsePool.GetResponse(correlationId);
            await Task.Delay(100);
        }
        if (response.Success) {
            return Ok(response.Result);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsersByIpPart([FromQuery] string ipPart, [FromQuery] string protocol)
    {
        ipPart = ipPart.Trim();
        if (string.IsNullOrEmpty(ipPart))
        {
            return BadRequest(new { message = "ipPart must not be empty" });
        }

        if (!IsValidIpProtocol(protocol))
        {
            return BadRequest(new { message = "Invalid protocol type" });
        }

        protocol = NormalizeIpProtocol(protocol);      

        if (!IsValidIpStart(ipPart, protocol))
        {
            return BadRequest(new { message = "Invalid ipPart or ipPart does not match the protocol type" });
        }

        var message = new SearchUsersByIpPartMessage()
        {
            Ip = ipPart,
            Protocol = protocol
        };

        var type = typeof(SearchUsersByIpPartMessage).Name;
        var headers = new Dictionary<string, object>
        {
            { "type",  type}
        };

        var correlationId = Guid.NewGuid().ToString();
        await _requestProduser.SendAsync(message, correlationId, headers);

        // Ожидание ответа
        ResponseResult response = null;
        while (response == null)
        {
            response = _responsePool.GetResponse(correlationId);
            await Task.Delay(100);
        }

        if (response.Success)
        {
            return Ok(response.Result);
        }
        else
        {
            return BadRequest(response);
        }
    }

    [HttpGet("{userId}/ips")]
    public async Task<IActionResult> GetUserIps(long userId)
    {
        var message = new GetUserIpsMessage()
        {
            UserId = userId
        };

        var type = typeof(GetUserIpsMessage).Name;
        var headers = new Dictionary<string, object>
        {
            { "type",  type}
        };

        var correlationId = Guid.NewGuid().ToString();
        await _requestProduser.SendAsync(message, correlationId, headers);

        // Ожидание ответа
        ResponseResult response = null;
        while (response == null)
        {
            response = _responsePool.GetResponse(correlationId);
            await Task.Delay(100);
        }

        if (response.Success)
        {
            return Ok(response.Result);
        }
        else
        {
            return BadRequest(response);
        }
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
            _ => "" // Если передан некорректный протокол, возвращаем ""
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
