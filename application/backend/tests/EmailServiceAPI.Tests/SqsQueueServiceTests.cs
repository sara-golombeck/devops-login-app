using Amazon.SQS;
using Amazon.SQS.Model;
using EmailServiceAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace EmailServiceAPI.Tests
{
    public class SqsQueueServiceTests
    {
        private readonly Mock<IAmazonSQS> _sqsClientMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<SqsQueueService>> _loggerMock;
        private readonly SqsQueueService _queueService;
        private readonly string _queueUrl = "https://sqs.region.amazonaws.com/123456789/test-queue";

        public SqsQueueServiceTests()
        {
            _sqsClientMock = new Mock<IAmazonSQS>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<SqsQueueService>>();

            _configurationMock.Setup(c => c["AWS:SQS:QueueUrl"]).Returns(_queueUrl);

            _queueService = new SqsQueueService(_sqsClientMock.Object, _configurationMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SendToEmailQueueAsync_ValidEmail_SendsMessageToSqs()
        {
            // Arrange
            var email = "test@example.com";
            var messageId = "test-message-id";
            
            _sqsClientMock
                .Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default))
                .ReturnsAsync(new SendMessageResponse { MessageId = messageId });

            // Act
            await _queueService.SendToEmailQueueAsync(email);

            // Assert
            _sqsClientMock.Verify(x => x.SendMessageAsync(
                It.Is<SendMessageRequest>(req => 
                    req.QueueUrl == _queueUrl &&
                    req.MessageBody.Contains(email) &&
                    req.MessageBody.Contains("Login")),
                default), Times.Once);
        }

        [Theory]
        [InlineData("user@domain.com")]
        [InlineData("test.email@company.org")]
        [InlineData("admin@test.co.il")]
        public async Task SendToEmailQueueAsync_VariousEmails_CreatesCorrectMessage(string email)
        {
            // Arrange
            _sqsClientMock
                .Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default))
                .ReturnsAsync(new SendMessageResponse { MessageId = "test-id" });

            // Act
            await _queueService.SendToEmailQueueAsync(email);

            // Assert
            _sqsClientMock.Verify(x => x.SendMessageAsync(
                It.Is<SendMessageRequest>(req => 
                    ValidateMessageContent(req.MessageBody, email)),
                default), Times.Once);
        }

        [Fact]
        public async Task SendToEmailQueueAsync_SqsThrowsException_PropagatesException()
        {
            // Arrange
            var email = "test@example.com";
            var exception = new Exception("SQS service unavailable");
            
            _sqsClientMock
                .Setup(x => x.SendMessageAsync(It.IsAny<SendMessageRequest>(), default))
                .ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<Exception>(() => 
                _queueService.SendToEmailQueueAsync(email));
            
            thrownException.Message.Should().Be("SQS service unavailable");
        }

        private static bool ValidateMessageContent(string messageBody, string expectedEmail)
        {
            try
            {
                var message = JsonSerializer.Deserialize<JsonElement>(messageBody);
                return message.GetProperty("Email").GetString() == expectedEmail &&
                       message.GetProperty("Type").GetString() == "Login" &&
                       message.TryGetProperty("Timestamp", out _);
            }
            catch
            {
                return false;
            }
        }
    }
}