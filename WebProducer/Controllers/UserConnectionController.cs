using Microsoft.AspNetCore.Mvc;
using MessageBrokerModelsLibrary.Models;
using System.Net;
using System.Net.Sockets;
using WebProducer.Services;
using IConsumerServiceMBT = MessageBrokerToolkit.Interfaces.IConsumerServiceMBT;
using IProduserServiceMBT = MessageBrokerToolkit.Interfaces.IProduserServiceMBT;
using Microsoft.Extensions.Options;
using WebProducer.Configurations;

[ApiController]
[Route("api/users/{userId}/connect")]
public class UserConnectionController : ControllerBase
{
    private readonly IProduserServiceMBT _produserService;
    private readonly IConsumerServiceMBT _consumerService;
    private readonly AppSettings _appSettings;

    public UserConnectionController(IProduserServiceMBT produserService, IConsumerServiceMBT consumerService, IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
        _produserService = produserService;
        _consumerService = consumerService;
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

        //await _produserService.SendAsync(message, correlationId);

        //var response = await _consumerService.WaitForReplyAsync(correlationId);

        //if (!_consumerService.IsListening)
        //{
        //    _consumerService.StartListening();
        //}

        var correlationId = Guid.NewGuid().ToString();
        await _produserService.SendAsync(request, correlationId, _appSettings.RabbitMQ.RequestQueue);

        await _consumerService.StartConsumingAsync(_appSettings.RabbitMQ.ResponseQueue);
        var response = await _consumerService.WaitForResponseAsync(correlationId);
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
