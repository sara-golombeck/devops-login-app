using EmailWorker.Models;
using System.Text.Json;

namespace EmailWorker.Tests;

public class ModelsTests
{
    [Fact]
    public void EmailMessage_DefaultValues_AreCorrect()
    {
        // Act
        var emailMessage = new EmailMessage();

        // Assert
        emailMessage.Email.Should().Be(string.Empty);
        emailMessage.Type.Should().Be(string.Empty);
        emailMessage.Timestamp.Should().Be(default(DateTime));
    }

    [Fact]
    public void EmailMessage_SetProperties_WorksCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var type = "login";
        var timestamp = DateTime.UtcNow;

        // Act
        var emailMessage = new EmailMessage
        {
            Email = email,
            Type = type,
            Timestamp = timestamp
        };

        // Assert
        emailMessage.Email.Should().Be(email);
        emailMessage.Type.Should().Be(type);
        emailMessage.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void EmailMessage_JsonSerialization_WorksCorrectly()
    {
        // Arrange
        var emailMessage = new EmailMessage
        {
            Email = "test@example.com",
            Type = "login",
            Timestamp = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var json = JsonSerializer.Serialize(emailMessage);
        var deserializedMessage = JsonSerializer.Deserialize<EmailMessage>(json);

        // Assert
        deserializedMessage.Should().NotBeNull();
        deserializedMessage!.Email.Should().Be(emailMessage.Email);
        deserializedMessage.Type.Should().Be(emailMessage.Type);
        deserializedMessage.Timestamp.Should().Be(emailMessage.Timestamp);
    }

    [Fact]
    public void EmailResult_DefaultValues_AreCorrect()
    {
        // Act
        var emailResult = new EmailResult();

        // Assert
        emailResult.Success.Should().BeFalse();
        emailResult.ErrorMessage.Should().BeNull();
        emailResult.MessageId.Should().BeNull();
    }

    [Fact]
    public void EmailResult_SuccessResult_WorksCorrectly()
    {
        // Arrange
        var messageId = "test-message-id";

        // Act
        var emailResult = new EmailResult
        {
            Success = true,
            MessageId = messageId
        };

        // Assert
        emailResult.Success.Should().BeTrue();
        emailResult.MessageId.Should().Be(messageId);
        emailResult.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void EmailResult_FailureResult_WorksCorrectly()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var emailResult = new EmailResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };

        // Assert
        emailResult.Success.Should().BeFalse();
        emailResult.ErrorMessage.Should().Be(errorMessage);
        emailResult.MessageId.Should().BeNull();
    }

    [Theory]
    [InlineData("user@domain.com", "login")]
    [InlineData("admin@company.co.il", "notification")]
    [InlineData("test@example.org", "alert")]
    public void EmailMessage_VariousInputs_WorkCorrectly(string email, string type)
    {
        // Act
        var emailMessage = new EmailMessage
        {
            Email = email,
            Type = type,
            Timestamp = DateTime.UtcNow
        };

        // Assert
        emailMessage.Email.Should().Be(email);
        emailMessage.Type.Should().Be(type);
        emailMessage.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}