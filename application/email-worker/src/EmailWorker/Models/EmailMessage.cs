namespace EmailWorker.Models
{
    public class EmailMessage
    {
        public string Email { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}