namespace DataLibrary.Models;

public class Connection
{
    public long UserId { get; init; }

    public User User { get; set; }

    public long IpAddressId { get; set; }

    public IpAddress IpAddress { get; set; }

    public DateTime ConnectedAt { get; set; } = DateTime.Now;
}
