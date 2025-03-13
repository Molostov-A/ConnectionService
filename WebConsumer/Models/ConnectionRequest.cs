namespace ConnectionService.Models
{
    public class ConnectionRequest
    {
        public long UserId { get; set; }
        public string IpAddress { get; set; }
        public string Protocol { get; set; }
    }
}