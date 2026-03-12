namespace GPCS_ExchangeRate.Infrastructure.Data;

/// <summary>
/// Interface for resolving the current user from the request context.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
}
