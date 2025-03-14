using Microsoft.AspNetCore.Mvc;
using WebProducer.Services;

namespace WebProducer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : Controller
    {
        public class ConnectionController : ControllerBase
        {
            private readonly IProduserService _produserService;

            public ConnectionController(IProduserService produserService)
            {
                _produserService = produserService;
            }

            [HttpPost("connect")]
            public IActionResult Connect([FromBody] ConnectionRequest request)
            {
                _produserService.SendMessageAsync(request).Wait();

                return Ok("Message sent");
            }

            [Route("api/users/{userId}/connect")]
            [ApiController]
            public class UserConnectionController : ControllerBase
            {
                private readonly AppDbContext _context;

                public UserConnectionController(AppDbContext context)
                {
                    _context = context;
                }

                [HttpPost]
                public async Task<IActionResult> ConnectUser(long userId, [FromBody] ConnectionRequest request)
                {
                    if (request == null || string.IsNullOrWhiteSpace(request.Ip))
                    {
                        return BadRequest("Invalid IP address.");
                    }

                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        return NotFound("User not found.");
                    }

                    var ipAddress = await _context.IpAddresses.FirstOrDefaultAsync(ip => ip.Address == request.Ip);
                    if (ipAddress == null)
                    {
                        ipAddress = new IpAddress { Address = request.Ip, Protocol = "IPv4" }; // Пример для IPv4
                        _context.IpAddresses.Add(ipAddress);
                        await _context.SaveChangesAsync();
                    }

                    var connection = new Connection
                    {
                        UserId = userId,
                        IpAddressId = ipAddress.Id,
                        ConnectedAt = DateTime.UtcNow
                    };

                    _context.Connections.Add(connection);
                    await _context.SaveChangesAsync();

                    return Ok("User connected successfully.");
                }
            }


        }
    }
}
