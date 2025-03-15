using Microsoft.AspNetCore.Mvc;
using MessageBrokerModelsLibrary.Models;
using System.Net;
using System.Net.Sockets;
using WebProducer;
using WebProducer.Interfaces;
using WebProducer.Controllers.Models;

[ApiController]
[Route("api/users/{userId}/connect")]
public class UserConnectionController : ControllerBase
{
    private readonly IRequestProduser _requestProduser;
    private readonly ResponsePool _responsePool;

    public UserConnectionController(IRequestProduser requestProduser, ResponsePool responsePool)
    {
        _requestProduser = requestProduser;
        _responsePool = responsePool;
    }

    [HttpPost]
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
}
