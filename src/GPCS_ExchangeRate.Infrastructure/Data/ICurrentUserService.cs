namespace GPCS_ExchangeRate.Infrastructure.Data;

/// <summary>
/// Interface สำหรับดึง current user ใน request context
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
}
