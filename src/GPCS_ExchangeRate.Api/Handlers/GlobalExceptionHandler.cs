using GPCS_ExchangeRate.Api.Models;
using GPCS_ExchangeRate.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using ValidationException = GPCS_ExchangeRate.Domain.Exceptions.ValidationException;

namespace GPCS_ExchangeRate.Api.Handlers;

/// <summary>
/// Global exception handler registered via IExceptionHandler (.NET 8+).
/// Maps domain and system exceptions to a consistent <see cref="ApiResponse{T}"/> envelope.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message, errorCode, errors) = MapException(exception);

        LogException(exception, statusCode);

        var response = ApiResponse.Fail(
            message:   message,
            errorCode: errorCode,
            errors:    errors,
            traceId:   context.TraceIdentifier);

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    // ?? Exception ? HTTP mapping ??????????????????????????????????????????????
    private static (HttpStatusCode status, string message, string errorCode, Dictionary<string, string[]>? errors)
        MapException(Exception ex) => ex switch
    {
        NotFoundException e =>
            (HttpStatusCode.NotFound, e.Message, "NOT_FOUND", null),

        ValidationException ve =>
            (HttpStatusCode.UnprocessableEntity, ve.Message, "VALIDATION_FAILED",
             ve.Errors.ToDictionary(k => k.Key, v => v.Value)),

        ForbiddenException e =>
            (HttpStatusCode.Forbidden, e.Message, "FORBIDDEN", null),

        UnauthorizedAccessException e =>
            (HttpStatusCode.Unauthorized, e.Message, "UNAUTHORIZED", null),

        ArgumentException e =>
            (HttpStatusCode.BadRequest, e.Message, "BAD_REQUEST", null),

        OperationCanceledException e =>
            (HttpStatusCode.BadRequest, e.Message, "REQUEST_CANCELLED", null),

        TimeoutException e =>
            (HttpStatusCode.GatewayTimeout, e.Message, "TIMEOUT", null),

        InvalidOperationException e =>
            (HttpStatusCode.UnprocessableEntity, e.Message, "OPERATION_FAILED", null),

        _ =>
            (HttpStatusCode.InternalServerError, "An unexpected error occurred.", "INTERNAL_ERROR", null)
    };

    // ?? Logging ???????????????????????????????????????????????????????????????
    private void LogException(Exception ex, HttpStatusCode statusCode)
    {
        if (statusCode >= HttpStatusCode.InternalServerError)
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
        else
            logger.LogWarning(ex, "Handled exception [{Status}]: {Message}", (int)statusCode, ex.Message);
    }
}
