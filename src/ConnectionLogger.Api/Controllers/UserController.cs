using ConnectionLogger.Data.Services;
using ConnectionLogger.Messaging.Messages;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    protected readonly ILogger<UserController> _logger;
    private readonly IDataService _dataService;
    public UserController(IDataService dataService, ILogger<UserController> logger)
    {
        _logger = logger;
        _dataService = dataService;
    }

    [HttpGet("{id}")]
    public IActionResult GetData(int id)
    {
        var response = new { Id = id, Message = $"Данные для ID {id}" };
        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsersByIpPart([FromQuery] string ipPart, [FromQuery] string protocol)
    {
        var message = new SearchUsersByIpPartMessage()
        {
            Ip = ipPart,
            Protocol = protocol
        };

        var result = await _dataService.GetUsersByIpAsync(message.Ip, message.Protocol);

        return Ok(result);
    }

    [HttpGet("{userId}/ips")]
    public async Task<IActionResult> GetUserIps(long userId)
    {
        var message = new GetUserIpsMessage()
        {
            UserId = userId
        };

        var result = await _dataService.GetUserIpsAsync(userId);

        return Ok(new { Response = result });
    }
}
