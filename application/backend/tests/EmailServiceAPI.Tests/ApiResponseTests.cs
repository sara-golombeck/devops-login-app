using EmailServiceAPI.Models;
using FluentAssertions;
using Xunit;

namespace EmailServiceAPI.Tests
{
    public class ApiResponseTests
    {
        [Fact]
        public void ApiResponse_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var response = new ApiResponse<string>();

            // Assert
            response.Success.Should().BeFalse();
            response.Message.Should().BeNull();
            response.Data.Should().Be(default(string));
            response.Errors.Should().BeNull();
        }

        [Fact]
        public void ApiResponse_WithSuccessData_SetsCorrectValues()
        {
            // Arrange
            var data = "test data";
            var message = "Operation successful";

            // Act
            var response = new ApiResponse<string>
            {
                Success = true,
                Message = message,
                Data = data
            };

            // Assert
            response.Success.Should().BeTrue();
            response.Message.Should().Be(message);
            response.Data.Should().Be(data);
            response.Errors.Should().BeNull();
        }

        [Fact]
        public void ApiResponse_WithErrors_SetsCorrectValues()
        {
            // Arrange
            var errors = new List<string> { "Error 1", "Error 2" };
            var message = "Validation failed";

            // Act
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Errors = errors
            };

            // Assert
            response.Success.Should().BeFalse();
            response.Message.Should().Be(message);
            response.Data.Should().BeNull();
            response.Errors.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public void ApiResponse_GenericType_WorksWithDifferentTypes()
        {
            // Act
            var stringResponse = new ApiResponse<string> { Data = "test" };
            var intResponse = new ApiResponse<int> { Data = 42 };
            var objectResponse = new ApiResponse<object> { Data = new { Id = 1, Name = "Test" } };

            // Assert
            stringResponse.Data.Should().Be("test");
            intResponse.Data.Should().Be(42);
            objectResponse.Data.Should().NotBeNull();
        }
    }
}