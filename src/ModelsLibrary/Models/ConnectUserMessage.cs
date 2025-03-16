namespace ModelsLibrary.Models;

public class ConnectUserMessage
{
    public long UserId { get; set; }

    public string IpAddress { get; set; }

    public string Protocol { get; set; }
}