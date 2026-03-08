using System;
using System.Collections.Generic;
using System.Text;
namespace Application
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public ErrorType? ErrorType { get; }
        public string ErrorMessage { get; }

        protected Result(bool isSuccess, T value, ErrorType? errorType, string errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorType = errorType;
            ErrorMessage = errorMessage;
        }

        // ── Success ──────────────────────────────────────────
        public static Result<T> Success(T value)
            => new Result<T>(true, value, null, null);

        // ── Failures (one per ErrorType) ──────────────────────
        public static Result<T> Failure(ErrorType? errorType, string message)
            => new Result<T>(false, default, errorType, message);

        public static Result<T> ValidationError(string message)
            => Failure(BLL.ErrorType.Validation, message);

        public static Result<T> NotFound(string message)
            => Failure(BLL.ErrorType.NotFound, message);

        public static Result<T> Conflict(string message)
            => Failure(BLL.ErrorType.Conflict, message);

        public static Result<T> Unauthorized(string message)
            => Failure(BLL.ErrorType.Unauthorized, message);

        public static Result<T> Forbidden(string message)
            => Failure(BLL.ErrorType.Forbidden, message);

        public static Result<T> BusinessRule(string message)
            => Failure(BLL.ErrorType.BusinessRule, message);

        public static Result<T> Unexpected(string message)
            => Failure(BLL.ErrorType.Unexpected, message);
    }
}