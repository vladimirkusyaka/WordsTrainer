using System.Net;

namespace WordsTrainer.Contracts.Common;

public sealed class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public HttpStatusCode? StatusCode { get; init; }

    public static ApiResult<T> Success(T? value)
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Value = value
        };
    }

    public static ApiResult<T> Failure(
        string? errorCode,
        string? errorMessage,
        HttpStatusCode? statusCode = null)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
}
