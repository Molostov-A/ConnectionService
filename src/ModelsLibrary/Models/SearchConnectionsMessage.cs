namespace ModelsLibrary.Models;

public class SearchConnectionsMessage
{
    public long UserId { get; set; }

    public OrderBy OrderBy { get; set; }

    public Direction Direction { get; set; }
}
