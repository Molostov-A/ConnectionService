using Microsoft.AspNetCore.Mvc;
using WebProducer.Interfaces;
using WebProducer;
using MessageBrokerModelsLibrary.Models;
using System;

[ApiController]
[Route("api/connections")]
public class ConnectionsController : ControllerBase
{
    private readonly IRequestProduser _requestProduser;
    private readonly ResponsePool _responsePool;

    public ConnectionsController(IRequestProduser requestProduser, ResponsePool responsePool)
    {
        _requestProduser = requestProduser;
        _responsePool = responsePool;
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

        var type = typeof(SearchConnectionsMessage).Name;
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
        if (response.Success)
        {
            return Ok(response.Result);
        }
        else
        {
            return BadRequest(response);
        }
    }
}
