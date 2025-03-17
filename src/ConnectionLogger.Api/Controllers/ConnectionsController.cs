using Microsoft.AspNetCore.Mvc;
using ConnectionLogger.Data.Services;
using ConnectionLogger.Messaging.Messages;
using System.Text.Json;
using ConnectionLogger.Data.Models;

[ApiController]
[Route("api/connections")]
public class ConnectionsController : ControllerBase
{
    protected readonly ILogger<UserController> _logger;
    private readonly IDataService _dataService;

    public ConnectionsController(IDataService dataService, ILogger<UserController> logger)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchConnections([FromQuery] long userId, [FromQuery] string orderBy = "dateCreated", [FromQuery] string direction = "desc")
    {
        if (!Enum.TryParse(orderBy, true, out OrderBy convertedOrderBy))
        {
            return BadRequest(new { message = $"orderBy must equal one of the following strings: {string.Join(", ", Enum.GetNames(typeof(OrderBy)))}" });
        }

        if (!Enum.TryParse(direction, true, out Direction convertedDirection))
        {
            return BadRequest(new { message = $"direction must equal one of the following strings: {string.Join(", ", Enum.GetNames(typeof(Direction)))}" });
        }

        var message = new SearchConnectionsMessage()
        {
            Direction = convertedDirection,
            OrderBy = convertedOrderBy,
            UserId = userId
        };

        var result = await _dataService.GetLatestConnectionAsync(message.UserId, message.OrderBy, message.Direction);
        var ip = await _dataService.GetAddressAsync(result.IpAddressId);
        var viewResult = new ConnectMessage()
        {
            UserId = result.UserId,
            ConnectedAt = result.ConnectedAt,
            Ip = ip.Address
        };

        string responseJson = JsonSerializer.Serialize(viewResult);

        return Ok(responseJson);
    }
}
