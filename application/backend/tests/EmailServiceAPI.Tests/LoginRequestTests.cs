using EmailServiceAPI.Models;
using FluentAssertions;
using Xunit;

namespace EmailServiceAPI.Tests
{
    public class LoginRequestTests
    {
        [Fact]
        public void LoginRequest_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var request = new LoginRequest();

            // Assert
            request.Email.Should().BeNull();
        }

        [Fact]
        public void LoginRequest_SetEmail_StoresValueCorrectly()
        {
            // Arrange
            var email = "test@example.com";

            // Act
            var request = new LoginRequest { Email = email };

            // Assert
            request.Email.Should().Be(email);
        }

        [Theory]
        [InlineData("user@domain.com")]
        [InlineData("test.email@company.org")]
        [InlineData("admin@test.co.il")]
        [InlineData("")]
        [InlineData(null)]
        public void LoginRequest_WithVariousEmails_StoresCorrectly(string email)
        {
            // Act
            var request = new LoginRequest { Email = email };

            // Assert
            request.Email.Should().Be(email);
        }

        [Fact]
        public void LoginRequest_EmailProperty_IsSettableAndGettable()
        {
            // Arrange
            var request = new LoginRequest();
            var email = "new@example.com";

            // Act
            request.Email = email;

            // Assert
            request.Email.Should().Be(email);
        }
    }
}