using System;
using Application;
using Xunit;

namespace Application.UnitTests
{
    public class ResultTests
    {
        [Fact]
        public void Success_WithValidValue_ReturnsSuccessfulResult()
        {
            // Arrange
            var value = "TestValue";

            // Act
            var result = global::Result<string>.Success(value);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(value, result.Value);
            Assert.Null(result.Error);
        }

        [Fact]
        public void Failure_WithError_ReturnsFailedResult()
        {
            // Arrange
            var error = Application.Error.Validation(400, "Validation failed");

            // Act
            var result = global::Result<string>.Failure(error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.Equal(error, result.Error);
        }

        [Fact]
        public void NotFound_WithCodeAndDescription_ReturnsNotFoundResult()
        {
            // Arrange
            int code = 404;
            string description = "Resource not found";

            // Act
            var result = global::Result<string>.NotFound(code, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal(Application.ErrorType.NotFound, result.Error.Type);
            Assert.Equal(code, result.Error.Code);
            Assert.Equal(description, result.Error.Description);
        }

        [Fact]
        public void IsFailure_WhenSuccess_ReturnsFalse()
        {
            // Act
            var result = global::Result<int>.Success(10);
            
            // Assert
            Assert.False(result.IsFailure);
        }

        [Fact]
        public void IsFailure_WhenFailure_ReturnsTrue()
        {
            // Arrange
            var error = Application.Error.Unexpected(500, "Unexpected error");

            // Act
            var result = global::Result<int>.Failure(error);
            
            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var expectedValue = 42;
            var error = Application.Error.Unexpected(500, "Oops");
            
            // Act
            var result = new TestResult<int>(true, expectedValue, error);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(expectedValue, result.Value);
            Assert.Equal(error, result.Error);
        }


        [Fact]
        public void Validation_WithCodeAndDescription_ReturnsValidationResult()
        {
            // Arrange
            int code = 400;
            string description = "Validation error occurred";

            // Act
            var result = global::Result<string>.Validation(code, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal(Application.ErrorType.Validation, result.Error.Type);
            Assert.Equal(code, result.Error.Code);
            Assert.Equal(description, result.Error.Description);
        }

        [Fact]
        public void Conflict_WithCodeAndDescription_ReturnsConflictResult()
        {
            // Arrange
            int code = 409;
            string description = "Conflict occurred";

            // Act
            var result = global::Result<string>.Conflict(code, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal(Application.ErrorType.Conflict, result.Error.Type);
            Assert.Equal(code, result.Error.Code);
            Assert.Equal(description, result.Error.Description);
        }

        [Fact]
        public void Unauthorized_WithCodeAndDescription_ReturnsUnauthorizedResult()
        {
            // Arrange
            int code = 401;
            string description = "Unauthorized access";

            // Act
            var result = global::Result<string>.Unauthorized(code, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal(Application.ErrorType.Unauthorized, result.Error.Type);
            Assert.Equal(code, result.Error.Code);
            Assert.Equal(description, result.Error.Description);
        }

        [Fact]
        public void Forbidden_WithCodeAndDescription_ReturnsForbiddenResult()
        {
            // Arrange
            int code = 403;
            string description = "Forbidden access";

            // Act
            var result = global::Result<string>.Forbidden(code, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal(Application.ErrorType.Forbidden, result.Error.Type);
            Assert.Equal(code, result.Error.Code);
            Assert.Equal(description, result.Error.Description);
        }

        [Fact]
        public void BusinessRule_WithCodeAndDescription_ReturnsBusinessRuleResult()
        {
            // Arrange
            int code = 422;
            string description = "Business rule violated";

            // Act
            var result = global::Result<string>.BusinessRule(code, description);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Null(result.Value);
            Assert.NotNull(result.Error);
            Assert.Equal(Application.ErrorType.BusinessRule, result.Error.Type);
            Assert.Equal(code, result.Error.Code);
            Assert.Equal(description, result.Error.Description);
        }

        private class TestResult<T> : global::Result<T>
        {
            public TestResult(bool isSuccess, T value, Application.Error error) 
                : base(isSuccess, value, error)
            {
            }
        }
    }
}