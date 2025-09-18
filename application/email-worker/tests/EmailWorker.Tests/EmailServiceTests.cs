using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using EmailWorker.Models;
using EmailWorker.Services;

namespace EmailWorker.Tests;

public class EmailServiceTests
{
    private readonly Mock<IAmazonSimpleEmailService> _sesClientMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<EmailService>> _loggerMock;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _sesClientMock = new Mock<IAmazonSimpleEmailService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<EmailService>>();

        _configurationMock.Setup(c => c["Email:FromAddress"]).Returns("test@example.com");
        _configurationMock.Setup(c => c["Email:Subject"]).Returns("Test Subject");

        _emailService = new EmailService(_sesClientMock.Object, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendLoginEmailAsync_ValidEmail_ReturnsSuccess()
    {
        // Arrange
        var email = "user@example.com";
        var messageId = "test-message-id";
        
        _sesClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ReturnsAsync(new SendEmailResponse { MessageId = messageId });

        // Act
        var result = await _emailService.SendLoginEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.MessageId.Should().Be(messageId);
        result.ErrorMessage.Should().BeNull();
    }

    [Theory]
    [InlineData("test@domain.com")]
    [InlineData("user.name@company.co.il")]
    [InlineData("admin@test.org")]
    public async Task SendLoginEmailAsync_VariousValidEmails_ReturnsSuccess(string email)
    {
        // Arrange
        _sesClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ReturnsAsync(new SendEmailResponse { MessageId = "test-id" });

        // Act
        var result = await _emailService.SendLoginEmailAsync(email);

        // Assert
        result.Success.Should().BeTrue();
        result.MessageId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SendLoginEmailAsync_SesThrowsException_ReturnsFailure()
    {
        // Arrange
        var email = "user@example.com";
        var errorMessage = "SES service unavailable";
        
        _sesClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _emailService.SendLoginEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.MessageId.Should().BeNull();
    }

    [Fact]
    public async Task SendLoginEmailAsync_ValidRequest_CallsSesWithCorrectParameters()
    {
        // Arrange
        var email = "user@example.com";
        var fromEmail = "sender@example.com";
        var subject = "Login Subject";
        
        _configurationMock.Setup(c => c["Email:FromAddress"]).Returns(fromEmail);
        _configurationMock.Setup(c => c["Email:Subject"]).Returns(subject);
        
        _sesClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ReturnsAsync(new SendEmailResponse { MessageId = "test-id" });

        // Act
        await _emailService.SendLoginEmailAsync(email);

        // Assert
        _sesClientMock.Verify(x => x.SendEmailAsync(
            It.Is<SendEmailRequest>(req => 
                req.Source == fromEmail &&
                req.Destination.ToAddresses.Contains(email) &&
                req.Message.Subject.Data == subject),
            default), Times.Once);
    }

    [Fact]
    public async Task SendLoginEmailAsync_UsesDefaultValues_WhenConfigurationMissing()
    {
        // Arrange
        var email = "user@example.com";
        _configurationMock.Setup(c => c["Email:FromAddress"]).Returns((string?)null);
        _configurationMock.Setup(c => c["Email:Subject"]).Returns((string?)null);
        
        _sesClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), default))
            .ReturnsAsync(new SendEmailResponse { MessageId = "test-id" });

        // Act
        await _emailService.SendLoginEmailAsync(email);

        // Assert
        _sesClientMock.Verify(x => x.SendEmailAsync(
            It.Is<SendEmailRequest>(req => 
                req.Source == "email@myname.click" &&
                req.Message.Subject.Data == "Login to Your Account"),
            default), Times.Once);
    }
}