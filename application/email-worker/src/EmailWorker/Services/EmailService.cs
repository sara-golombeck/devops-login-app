using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using EmailWorker.Models;

namespace EmailWorker.Services
{
    public class EmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _sesClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IAmazonSimpleEmailService sesClient,
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _sesClient = sesClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<EmailResult> SendLoginEmailAsync(string emailAddress)
        {
            try
            {
                var fromEmail = _configuration["Email:FromAddress"] ?? "email@myname.click";
                var subject = _configuration["Email:Subject"] ?? "Login to Your Account";
                
                var request = new SendEmailRequest
                {
                    Source = fromEmail,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { emailAddress }
                    },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Text = new Content($"Hello! You have successfully logged into the system at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC")
                        }
                    }
                };

                var response = await _sesClient.SendEmailAsync(request);
                
                _logger.LogInformation("Email sent successfully to: {Email}, MessageId: {MessageId}", 
                    emailAddress, response.MessageId);

                return new EmailResult
                {
                    Success = true,
                    MessageId = response.MessageId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to: {Email}", emailAddress);
                return new EmailResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}