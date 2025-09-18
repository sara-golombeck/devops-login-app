using EmailWorker.Models;

namespace EmailWorker.Services
{
    public interface IEmailService
    {
        Task<EmailResult> SendLoginEmailAsync(string emailAddress);
    }
}