using ConnectionService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using WebProducer.Models;
using WebProducer.Services;

[ApiController]
[Route("api/users/{userId}/connect")]
public class UserConnectionController : ControllerBase
{
    private readonly IProduserService _produserService;

    public UserConnectionController(IProduserService produserService)
    {
        _produserService = produserService;
    }

    [HttpPost]
    public async Task<IActionResult> ConnectUser(long userId, [FromBody] UserConnectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Ip) || !IsValidIp(request.Ip))
        {
            return BadRequest(new { message = "Invalid IP address" });
        }

        string protocol = GetIpProtocol(request.Ip);

        var message = new UserConnectionMessage
        {
            UserId = userId,
            IpAddress = request.Ip,
            Protocol = protocol
        };

        await _produserService.SendMessageAsync(message);

        var response = new UserConnectionResponse
        {
            UserId = userId,
            IpAddress = request.Ip,
            Protocol = protocol
        };

        return Ok(response);
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
