using Xunit;
using Application;

namespace Application.UnitTests;

public class ErrorTypeTests
{
    [Fact]
    public void Validation_WithValidInputs_ShouldReturnValidationError()
    {
        // Arrange
        var code = 400;
        var description = "Validation Error Description";

        // Act
        var result = Error.Validation(code, description);

        // Assert
        Assert.Equal(ErrorType.Validation, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }

    [Fact]
    public void NotFound_WithValidInputs_ShouldReturnNotFoundError()
    {
        // Arrange
        var code = 404;
        var description = "Not Found Error Description";

        // Act
        var result = Error.NotFound(code, description);

        // Assert
        Assert.Equal(ErrorType.NotFound, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }

    [Fact]
    public void Conflict_WithValidInputs_ShouldReturnConflictError()
    {
        // Arrange
        var code = 409;
        var description = "Conflict Error Description";

        // Act
        var result = Error.Conflict(code, description);

        // Assert
        Assert.Equal(ErrorType.Conflict, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }

    [Fact]
    public void Unauthorized_WithValidInputs_ShouldReturnUnauthorizedError()
    {
        // Arrange
        var code = 401;
        var description = "Unauthorized Error Description";

        // Act
        var result = Error.Unauthorized(code, description);

        // Assert
        Assert.Equal(ErrorType.Unauthorized, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }

    [Fact]
    public void Forbidden_WithValidInputs_ShouldReturnForbiddenError()
    {
        // Arrange
        var code = 403;
        var description = "Forbidden Error Description";

        // Act
        var result = Error.Forbidden(code, description);

        // Assert
        Assert.Equal(ErrorType.Forbidden, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }

    [Fact]
    public void BusinessRule_WithValidInputs_ShouldReturnBusinessRuleError()
    {
        // Arrange
        var code = 422;
        var description = "Business Rule Error Description";

        // Act
        var result = Error.BusinessRule(code, description);

        // Assert
        Assert.Equal(ErrorType.BusinessRule, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }

    [Fact]
    public void Unexpected_WithValidInputs_ShouldReturnUnexpectedError()
    {
        // Arrange
        var code = 500;
        var description = "Unexpected Error Description";

        // Act
        var result = Error.Unexpected(code, description);

        // Assert
        Assert.Equal(ErrorType.Unexpected, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }

    [Fact]
    public void InvalidOperation_WithValidInputs_ShouldReturnInvalidOperationError()
    {
        // Arrange
        var code = 400;
        var description = "Invalid Operation Error Description";

        // Act
        var result = Error.InvalidOperation(code, description);

        // Assert
        Assert.Equal(ErrorType.InvalidOperation, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }

    [Fact]
    public void AlreadyExists_WithValidInputs_ShouldReturnAlreadyExistsError()
    {
        // Arrange
        var code = 409;
        var description = "Already Exists Error Description";

        // Act
        var result = Error.AlreadyExists(code, description);

        // Assert
        Assert.Equal(ErrorType.AlreadyExists, result.Type);
        Assert.Equal(code, result.Code);
        Assert.Equal(description, result.Description);
    }
}
