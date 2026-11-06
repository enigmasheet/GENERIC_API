using System.Diagnostics;

namespace ModularMonolith.Infrastructure.Api;

public sealed record ApiResponse<T>
{
    public bool Success { get; init; } = true;

    public string? Message { get; init; }

    public T? Data { get; init; }

    public string? TraceId { get; init; } = Activity.Current?.TraceId.ToString();

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Message = message
        };
    }
}
