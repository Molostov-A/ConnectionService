using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using ConnectionService.Models;
using WebProducer.Services;

namespace ConnectionService.Controllers
{
    [ApiController]
    [Route("api/connections")]
    public class ConnectionController : ControllerBase
    {
        private readonly IProduserService _produserService;

        public ConnectionController(IProduserService produserService)
        {
            _produserService = produserService;
        }

        [HttpPost("connect")]
        public IActionResult Connect([FromBody] UserConnectionMessage request)
        {
            _produserService.SendMessageAsync(request).Wait();

            return Ok("Message sent");
        }
    }
}
