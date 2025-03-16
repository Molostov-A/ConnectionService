using Microsoft.AspNetCore.Mvc;
using MessageBrokerModelsLibrary.Models;
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
        if (request == null || !IsValidIp(request.ip))
        {
            return BadRequest(new { message = "Invalid IP address" });
        }

        string ip = request.ip;

        string protocol = GetIpProtocol(ip);

        var message = new UserConnectionMessage
        {
            UserId = userId,
            IpAddress = ip,
            Protocol = protocol
        };

        var type = typeof(UserConnectionMessage).Name;
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

        if (!IsValidIpFragment(ipPart, protocol))
        {
            return BadRequest(new { message = "Invalid ipPart or ipPart does not match the protocol type" });
        }

        var message = new IpMessage()
        {
            Ip = ipPart,
            Protocol = protocol
        };

        //// Логика поиска пользователей по ipPart и protocol
        //var users = await _requestProduser.SearchUsersByIp(ipPart, protocol);

        //if (users != null)
        //{
        //    return Ok(users);
        //}
        //else
        //{
        //    return NotFound(new { message = "No users found with the specified IP part and version" });
        //}

        return Ok(message);
    }



    [HttpGet("{userId}/ips")]
    public async Task<IActionResult> GetUserIps(long userId)
    {
        //var ips = await _requestProduser.GetUserIps(userId);

        //if (ips != null && ips.Any())
        //{
        //    return Ok(ips);
        //}
        //else
        //{
        //    return NotFound(new { message = "No IP addresses found for this user" });
        //}

        return Ok();
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

    private bool IsValidIpFragment(string segment, string protocol)
    {
        if (protocol.Equals("IPv4", StringComparison.OrdinalIgnoreCase))
        {
            return IsValidIPv4Fragment(segment);
        }
        else if (protocol.Equals("IPv6", StringComparison.OrdinalIgnoreCase))
        {
            return IsValidIPv6Fragment(segment);
        }
        return false;
    }

    private bool IsValidIPv4Fragment(string segment)
    {
        // Проверка, что это либо одно число (0-255), либо несколько октетов (X.X.X)
        if (Regex.IsMatch(segment, @"^(\d{1,3}\.){0,2}\d{1,3}$"))
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

    private bool IsValidIPv6Fragment(string segment)
    {
        // Проверка на один блок (0000-FFFF)
        if (Regex.IsMatch(segment, @"^[0-9a-fA-F]{1,4}$"))
            return true;

        // Проверка на несколько блоков IPv6 (X:X, X:X:X, X:X:X:X)
        if (Regex.IsMatch(segment, @"^([0-9a-fA-F]{1,4}:){0,3}[0-9a-fA-F]{1,4}$"))
            return true;

        // Исправленная проверка на "::" и "fe80::" (или другие аналогичные)
        if (Regex.IsMatch(segment, @"^([0-9a-fA-F]{1,4}::)$"))
            return true;

        // Проверка на "::" (сокращение IPv6)
        if (segment == "::")
            return true;

        return false;
    }

}
