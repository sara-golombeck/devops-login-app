using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace EmailServiceAPI.Services
{
    public class SqsQueueService : IQueueService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SqsQueueService> _logger;

        public SqsQueueService(IAmazonSQS sqsClient, IConfiguration configuration, ILogger<SqsQueueService> logger)
        {
            _sqsClient = sqsClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendToEmailQueueAsync(string email)
        {
            var queueUrl = _configuration["AWS:SQS:QueueUrl"];
            
            var message = new
            {
                Email = email,
                Type = "Login",
                Timestamp = DateTime.UtcNow
            };

            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = JsonSerializer.Serialize(message)
            };

            var response = await _sqsClient.SendMessageAsync(request);
            
            _logger.LogInformation("Message sent to SQS. MessageId: {MessageId}, Email: {Email}", 
                response.MessageId, email);
        }
    }
}