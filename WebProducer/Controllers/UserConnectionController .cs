using ConnectionService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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
    public async Task<IActionResult> ConnectUser(long userId, [FromBody] JsonObject request)
    {
        if (!request.TryGetPropertyValue("ip", out var ipNode) || ipNode is null)
        {
            return BadRequest(new { message = "IP address is missing or invalid format" });
        }

        string? ip = ipNode.GetValue<string?>();

        if (!IsValidIp(ip))
        {
            return BadRequest(new { message = "Invalid IP address" });
        }

        string protocol = GetIpProtocol(ip);

        var message = new UserConnectionMessage
        {
            UserId = userId,
            IpAddress = ip,
            Protocol = protocol
        };

        await _produserService.SendMessageAsync(message);

        return Ok(message);
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
