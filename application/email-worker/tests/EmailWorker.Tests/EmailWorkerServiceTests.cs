using Amazon.SQS;
using Amazon.SQS.Model;
using EmailWorker.Models;
using EmailWorker.Services;
using System.Text.Json;

namespace EmailWorker.Tests;

public class EmailWorkerServiceTests
{
    private readonly Mock<IAmazonSQS> _sqsClientMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<EmailWorkerService>> _loggerMock;
    private readonly EmailWorkerService _workerService;
    private readonly string _queueUrl = "https://sqs.region.amazonaws.com/123456789/test-queue";

    public EmailWorkerServiceTests()
    {
        _sqsClientMock = new Mock<IAmazonSQS>();
        _emailServiceMock = new Mock<IEmailService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailWorkerService>>();

        _configurationMock.Setup(c => c["AWS:SQS:QueueUrl"]).Returns(_queueUrl);

        _workerService = new EmailWorkerService(
            _sqsClientMock.Object,
            _emailServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void Constructor_MissingQueueUrl_ThrowsException()
    {
        // Arrange
        _configurationMock.Setup(c => c["AWS:SQS:QueueUrl"]).Returns((string?)null);

        // Act & Assert
        var action = () => new EmailWorkerService(
            _sqsClientMock.Object,
            _emailServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("SQS Queue URL not configured");
    }

    [Fact]
    public async Task ProcessMessage_ValidEmailMessage_SendsEmailSuccessfully()
    {
        // Arrange
        var emailMessage = new EmailMessage
        {
            Email = "test@example.com",
            Type = "login",
            Timestamp = DateTime.UtcNow
        };
        
        var message = new Message
        {
            Body = JsonSerializer.Serialize(emailMessage),
            MessageId = "test-message-id",
            ReceiptHandle = "test-receipt-handle"
        };

        var emailResult = new EmailResult { Success = true, MessageId = "ses-message-id" };
        
        _emailServiceMock
            .Setup(x => x.SendLoginEmailAsync(emailMessage.Email))
            .ReturnsAsync(emailResult);

        _sqsClientMock
            .Setup(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default))
            .ReturnsAsync(new DeleteMessageResponse());

        // Act
        var method = typeof(EmailWorkerService).GetMethod("ProcessMessageAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(_workerService, new object[] { message, CancellationToken.None })!;

        // Assert
        _emailServiceMock.Verify(x => x.SendLoginEmailAsync(emailMessage.Email), Times.Once);
        _sqsClientMock.Verify(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default), Times.Once);
    }

    [Fact]
    public async Task ProcessMessage_EmailServiceFails_DeletesMessage()
    {
        // Arrange
        var emailMessage = new EmailMessage
        {
            Email = "test@example.com",
            Type = "login",
            Timestamp = DateTime.UtcNow
        };
        
        var message = new Message
        {
            Body = JsonSerializer.Serialize(emailMessage),
            MessageId = "test-message-id",
            ReceiptHandle = "test-receipt-handle"
        };

        var emailResult = new EmailResult { Success = false, ErrorMessage = "SES error" };
        
        _emailServiceMock
            .Setup(x => x.SendLoginEmailAsync(emailMessage.Email))
            .ReturnsAsync(emailResult);

        _sqsClientMock
            .Setup(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default))
            .ReturnsAsync(new DeleteMessageResponse());

        // Act
        var method = typeof(EmailWorkerService).GetMethod("ProcessMessageAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(_workerService, new object[] { message, CancellationToken.None })!;

        // Assert
        _emailServiceMock.Verify(x => x.SendLoginEmailAsync(emailMessage.Email), Times.Once);
        _sqsClientMock.Verify(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default), Times.Once);
    }

    [Fact]
    public async Task ProcessMessage_InvalidJson_DeletesMessage()
    {
        // Arrange
        var message = new Message
        {
            Body = "invalid json content",
            MessageId = "test-message-id",
            ReceiptHandle = "test-receipt-handle"
        };

        _sqsClientMock
            .Setup(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default))
            .ReturnsAsync(new DeleteMessageResponse());

        // Act
        var method = typeof(EmailWorkerService).GetMethod("ProcessMessageAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(_workerService, new object[] { message, CancellationToken.None })!;

        // Assert
        _emailServiceMock.Verify(x => x.SendLoginEmailAsync(It.IsAny<string>()), Times.Never);
        _sqsClientMock.Verify(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default), Times.Once);
    }

    [Fact]
    public async Task ProcessMessage_EmptyEmail_DeletesMessage()
    {
        // Arrange
        var emailMessage = new EmailMessage
        {
            Email = "",
            Type = "login",
            Timestamp = DateTime.UtcNow
        };
        
        var message = new Message
        {
            Body = JsonSerializer.Serialize(emailMessage),
            MessageId = "test-message-id",
            ReceiptHandle = "test-receipt-handle"
        };

        _sqsClientMock
            .Setup(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default))
            .ReturnsAsync(new DeleteMessageResponse());

        // Act
        var method = typeof(EmailWorkerService).GetMethod("ProcessMessageAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(_workerService, new object[] { message, CancellationToken.None })!;

        // Assert
        _emailServiceMock.Verify(x => x.SendLoginEmailAsync(It.IsAny<string>()), Times.Never);
        _sqsClientMock.Verify(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default), Times.Once);
    }

    [Fact]
    public async Task ProcessMessage_EmailServiceThrowsException_DoesNotDeleteMessage()
    {
        // Arrange
        var emailMessage = new EmailMessage
        {
            Email = "test@example.com",
            Type = "login",
            Timestamp = DateTime.UtcNow
        };
        
        var message = new Message
        {
            Body = JsonSerializer.Serialize(emailMessage),
            MessageId = "test-message-id",
            ReceiptHandle = "test-receipt-handle"
        };

        _emailServiceMock
            .Setup(x => x.SendLoginEmailAsync(emailMessage.Email))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var method = typeof(EmailWorkerService).GetMethod("ProcessMessageAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(_workerService, new object[] { message, CancellationToken.None })!;

        // Assert
        _emailServiceMock.Verify(x => x.SendLoginEmailAsync(emailMessage.Email), Times.Once);
        _sqsClientMock.Verify(x => x.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteMessage_Success_CallsSqsDelete()
    {
        // Arrange
        var message = new Message
        {
            MessageId = "test-message-id",
            ReceiptHandle = "test-receipt-handle"
        };

        _sqsClientMock
            .Setup(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default))
            .ReturnsAsync(new DeleteMessageResponse());

        // Act
        var method = typeof(EmailWorkerService).GetMethod("DeleteMessageAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(_workerService, new object[] { message })!;

        // Assert
        _sqsClientMock.Verify(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default), Times.Once);
    }

    [Fact]
    public async Task DeleteMessage_SqsThrowsException_LogsError()
    {
        // Arrange
        var message = new Message
        {
            MessageId = "test-message-id",
            ReceiptHandle = "test-receipt-handle"
        };

        _sqsClientMock
            .Setup(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default))
            .ThrowsAsync(new Exception("SQS error"));

        // Act
        var method = typeof(EmailWorkerService).GetMethod("DeleteMessageAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(_workerService, new object[] { message })!;

        // Assert
        _sqsClientMock.Verify(x => x.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, default), Times.Once);
    }
}