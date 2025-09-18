using EmailServiceAPI.Models;
using FluentAssertions;
using Xunit;

namespace EmailServiceAPI.Tests
{
    public class UserModelTests
    {
        [Fact]
        public void User_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var user = new User();

            // Assert
            user.Id.Should().Be(0);
            user.Email.Should().BeNull();
            user.LoginAttempts.Should().Be(0);
            user.LastLoginAttempt.Should().Be(default(DateTime));
        }

        [Fact]
        public void User_SetProperties_StoresValuesCorrectly()
        {
            // Arrange
            var email = "test@example.com";
            var loginAttempts = 5;
            var lastLogin = DateTime.UtcNow;

            // Act
            var user = new User
            {
                Email = email,
                LoginAttempts = loginAttempts,
                LastLoginAttempt = lastLogin
            };

            // Assert
            user.Email.Should().Be(email);
            user.LoginAttempts.Should().Be(loginAttempts);
            user.LastLoginAttempt.Should().Be(lastLogin);
        }

        [Theory]
        [InlineData("user@domain.com", 1)]
        [InlineData("test@example.org", 10)]
        [InlineData("admin@company.co.il", 100)]
        public void User_WithVariousData_StoresCorrectly(string email, int attempts)
        {
            // Act
            var user = new User
            {
                Email = email,
                LoginAttempts = attempts
            };

            // Assert
            user.Email.Should().Be(email);
            user.LoginAttempts.Should().Be(attempts);
        }

        [Fact]
        public void User_IncrementLoginAttempts_UpdatesCorrectly()
        {
            // Arrange
            var user = new User { LoginAttempts = 3 };

            // Act
            user.LoginAttempts++;

            // Assert
            user.LoginAttempts.Should().Be(4);
        }
    }
}