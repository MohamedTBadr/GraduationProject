using System;
using System.Collections.Generic;
using System.Text;

namespace BLL
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public ErrorType? Error { get; }
        public T Value { get; }

        protected Result(bool isSuccess, T value, ErrorType? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value)
            => new Result<T>(true, value, null);

        public static Result<T> Failure(ErrorType error)
            => new Result<T>(false, default, error);
    }
}
