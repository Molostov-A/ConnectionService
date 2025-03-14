namespace WebProducer.Models
{
    public class UserConnectionResponse
    {
        public long UserId { get; set; }
        public string IpAddress { get; set; }
        public string Protocol { get; set; }
        public string CorrelationId { get; set; }
    }
}
