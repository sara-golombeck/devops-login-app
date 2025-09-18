using EmailServiceAPI.Models;
using EmailServiceAPI.Validators;
using FluentAssertions;
using Xunit;

namespace EmailServiceAPI.Tests
{
    public class ValidatorTests
    {
        private readonly LoginRequestValidator _validator;

        public ValidatorTests()
        {
            _validator = new LoginRequestValidator();
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user@domain.org")]
        [InlineData("valid.email@test.co.uk")]
        public async Task Validate_ValidEmails_ShouldPass(string email)
        {
            // Arrange
            var request = new LoginRequest { Email = email };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email")]
        [InlineData("@missing-user.com")]
        [InlineData("missing-domain@")]
        public async Task Validate_InvalidEmails_ShouldFail(string email)
        {
            // Arrange
            var request = new LoginRequest { Email = email };

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }
    }
}