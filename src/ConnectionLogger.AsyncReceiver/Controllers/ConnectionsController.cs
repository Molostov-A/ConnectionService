using ConnectionLogger.Messaging.Messages;
using ConnectionLogger.AsyncReceiver;
using ConnectionLogger.AsyncReceiver.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ConnectionLogger.AsyncReceiver.Services;

[ApiController]
[Route("api/connections")]
public class ConnectionsController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IApiService _apiService;

    public ConnectionsController(ILogger<UserController> logger, IApiService userService)
    {
        _logger = logger;
        _apiService = userService;
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

        var pathUrl = $"api/connections/search?userId={userId}&orderBy={orderBy}&direction={direction}";
        var result = await _apiService.GetDataFromServer(pathUrl);

        _logger.LogInformation("Search completed, found {ResultCount} records", result.Length);
        return Ok(new { Response = result });
    }
}
