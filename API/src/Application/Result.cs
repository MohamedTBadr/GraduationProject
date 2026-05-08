using Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public Error? Error { get; }  // ← single property replaces ErrorType + ErrorMessage

    protected Result(bool isSuccess, T value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value)
        => new(true, value, null);

    public static Result<T> Failure(Error error)
        => new(false, default, error);

    // ── Shortcuts ────────────────────────────────────────────
    public static Result<T> NotFound(int code, string description)
        => Failure(Error.NotFound(code, description));

    public static Result<T> Validation(int code, string description)
        => Failure(Error.Validation(code, description));

    public static Result<T> Conflict(int code, string description)
        => Failure(Error.Conflict(code, description));

    public static Result<T> Unauthorized(int code, string description)
        => Failure(Error.Unauthorized(code, description));

    public static Result<T> Forbidden(int code, string description)
        => Failure(Error.Forbidden(code, description));

    public static Result<T> BusinessRule(int code, string description)
        => Failure(Error.BusinessRule(code, description));

    public static Result<T> Unexpected(int code, string description)
        => Failure(Error.Unexpected(code, description));

    public static Result<T> InvalidOperation(int code, string description)
        => Failure(Error.InvalidOperation(code, description));
     
}

// ── ResultExtensions.cs ──────────────────────────────────────────────────────

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result);

        int statusCode = result.Error?.Type switch
        {
            // Validation & Input
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.InvalidOperation => StatusCodes.Status400BadRequest,

            // Resource Errors
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.AlreadyExists => StatusCodes.Status409Conflict,
            ErrorType.Conflict => StatusCodes.Status409Conflict,

            // Authorization & Authentication
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,

            // Business Rule Violations
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Expired => StatusCodes.Status410Gone,
            ErrorType.LimitExceeded => StatusCodes.Status429TooManyRequests,
            ErrorType.InsufficientFunds => StatusCodes.Status422UnprocessableEntity,

            // External Dependencies
            ErrorType.ExternalService => StatusCodes.Status502BadGateway,
            ErrorType.Timeout => StatusCodes.Status504GatewayTimeout,

            // System / Infrastructure
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            ErrorType.NotImplemented => StatusCodes.Status501NotImplemented,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,

            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(new
        {
            result.IsSuccess,
            ErrorCode = result.Error?.Code,
            ErrorType = result.Error?.Type.ToString(),
            ErrorDescription = result.Error?.Description
        })
        { StatusCode = statusCode };
    }

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        string actionName,
        object routeValues)
    {
        if (result.IsSuccess)
            return new CreatedAtActionResult(actionName, null, routeValues, result);

        return result.ToActionResult();
    }
}