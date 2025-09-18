namespace EmailWorker.Models
{
    public class EmailResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? MessageId { get; set; }
    }
}