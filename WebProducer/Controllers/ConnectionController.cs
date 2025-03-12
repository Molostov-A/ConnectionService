using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using ConnectionService.Models;

namespace ConnectionService.Controllers
{
    [ApiController]
    [Route("api/connections")]
    public class ConnectionController : Controller
    {

        [HttpPost("connect")]
        public IActionResult Connect([FromBody] ConnectionRequest request)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672, // стандартный порт RabbitMQ
                UserName = "guest",
                Password = "guest"
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "connections",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var message = JsonSerializer.Serialize(request);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "",
                routingKey: "connections",
                basicProperties: null,
                body: body
            );

            return Ok("Message sent");
        }
    }
}
