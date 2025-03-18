using ConnectionLogger.AsyncReceiver.Controllers.Models;
using ConnectionLogger.AsyncReceiver.Interfaces;
using ConnectionLogger.Messaging.Messages;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Sockets;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IRequestProducer _requestProduser;
    private const string IPv4 = "IPv4";
    private const string IPv6 = "IPv6";
    private const string Unknown = "Unknown";
    private const string Invalid = "Invalid";

    public UserController(IRequestProducer requestProduser)
    {
        _requestProduser = requestProduser;
    }

    [HttpPost]
    [Route("{userId}/connect")]
    public async Task<IActionResult> Connect(long userId, [FromBody] Connection request)
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

    private string GetIpProtocol(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out var ip) ? ip.AddressFamily switch
        {
            AddressFamily.InterNetwork => IPv4,
            AddressFamily.InterNetworkV6 => IPv6,
            _ => Unknown
        } : Invalid;
    }

    private bool IsValidIp(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out _);
    }
}
