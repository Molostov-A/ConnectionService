using Microsoft.AspNetCore.Mvc;
using MessageBrokerModelsLibrary.Models;
using System.Net;
using System.Net.Sockets;
using WebProducer;
using WebProducer.Interfaces;
using WebProducer.Controllers.Models;

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

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsersByIpPart([FromQuery] string ipPart, [FromQuery] string ipVersion)
    {
        //if (string.IsNullOrEmpty(ipPart) || string.IsNullOrEmpty(ipVersion))
        //{
        //    return BadRequest(new { message = "Both ipPart and ipVersion are required" });
        //}

        //// Логика поиска пользователей по ipPart и ipVersion
        //var users = await _requestProduser.SearchUsersByIp(ipPart, ipVersion);

        //if (users != null)
        //{
        //    return Ok(users);
        //}
        //else
        //{
        //    return NotFound(new { message = "No users found with the specified IP part and version" });
        //}

        return Ok();
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

}
