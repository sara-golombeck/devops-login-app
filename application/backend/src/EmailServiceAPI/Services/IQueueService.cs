namespace EmailServiceAPI.Services
{
    public interface IQueueService
    {
        Task SendToEmailQueueAsync(string email);
    }
}