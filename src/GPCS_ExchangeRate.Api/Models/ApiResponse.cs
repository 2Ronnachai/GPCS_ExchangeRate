using System.Text.Json.Serialization;

namespace GPCS_ExchangeRate.Api.Models;

/// <summary>Generic success/failure envelope for all API responses.</summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Errors { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(
        string message,
        string? errorCode = null,
        Dictionary<string, string[]>? errors = null,
        string? traceId = null)
        => new() { Success = false, Message = message, ErrorCode = errorCode, Errors = errors, TraceId = traceId };
}

/// <summary>Non-generic overload for responses that carry no data payload.</summary>
public static class ApiResponse
{
    public static ApiResponse<object> Ok(string message = "")
        => new() { Success = true, Message = message };

    public static ApiResponse<object> Fail(
        string message,
        string? errorCode = null,
        Dictionary<string, string[]>? errors = null,
        string? traceId = null)
        => new() { Success = false, Message = message, ErrorCode = errorCode, Errors = errors, TraceId = traceId };
}
