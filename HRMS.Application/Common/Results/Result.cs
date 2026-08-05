using System.Text.Json.Serialization;

namespace HRMS.Application.Common.Results
{
    /// <summary>
    /// Represents the classification of domain and application errors.
    /// </summary>
    public enum ErrorType
    {
        Failure = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3,
        Unauthorized = 4,
        Forbidden = 5,
        Unexpected = 6
    }

    /// <summary>
    /// Represents a detailed error structure for application operations.
    /// </summary>
    public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
        public static readonly Error NullValue = new("Error.NullValue", "The specified value is null.", ErrorType.Validation);

        public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
        public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
        public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
        public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
        public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
        public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
    }

    /// <summary>
    /// Represents the result of an operation, indicating success or failure.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException("A successful result cannot contain an error.");
            }

            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException("A failed result must contain an error.");
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);

        public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
        public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
        public static Result<TValue> Create<TValue>(TValue? value) => value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
    }

    /// <summary>
    /// Represents the result of an operation carrying a payload value upon success.
    /// </summary>
    /// <typeparam name="TValue">The payload type.</typeparam>
    public class Result<TValue> : Result
    {
        private readonly TValue? _value;

        [JsonConstructor]
        internal Result(TValue? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public TValue Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

        public static implicit operator Result<TValue>(TValue? value) => Create(value);
    }
}
