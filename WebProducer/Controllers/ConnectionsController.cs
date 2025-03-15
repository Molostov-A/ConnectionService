using Microsoft.AspNetCore.Mvc;
using WebProducer.Interfaces;
using WebProducer;

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
    public async Task<IActionResult> SearchConnections([FromQuery] long userId, [FromQuery] string orderBy = "dateCreated", [FromQuery] string direction = "asc")
    {
        // Валидация параметров
        //if (string.IsNullOrEmpty(orderBy) || string.IsNullOrEmpty(direction))
        //{
        //    return BadRequest(new { message = "Both orderBy and direction are required." });
        //}

        //if (direction.ToLower() != "asc" && direction.ToLower() != "desc")
        //{
        //    return BadRequest(new { message = "Invalid direction. Use 'asc' or 'desc'." });
        //}

        //// Получение подключений с сортировкой
        //var connections = await _connectionService.SearchConnections(userId, orderBy, direction);

        //if (connections != null && connections.Any())
        //{
        //    return Ok(connections);
        //}
        //else
        //{
        //    return NotFound(new { message = "No connections found for the specified user." });
        //}

        return Ok();
    }
}
