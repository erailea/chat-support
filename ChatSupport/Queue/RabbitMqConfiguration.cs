namespace ChatSupport.Queue
{
    public class RabbitMqConfiguration
    {
        public required string HostName { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}