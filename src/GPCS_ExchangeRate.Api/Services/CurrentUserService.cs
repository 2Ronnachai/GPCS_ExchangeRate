using GPCS_ExchangeRate.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace GPCS_ExchangeRate.Api.Services;

/// <summary>
/// Implementation of ICurrentUserService that resolves the user from HttpContext.
/// Replace the resolution logic to match the actual auth mechanism (e.g. JWT claims).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.Identity?.Name
        ?? _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault()
        ?? "anonymous";
}
