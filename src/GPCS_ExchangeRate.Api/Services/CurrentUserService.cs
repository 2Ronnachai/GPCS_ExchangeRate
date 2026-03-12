using GPCS_ExchangeRate.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace GPCS_ExchangeRate.Api.Services;

/// <summary>
/// Implementation ของ ICurrentUserService ที่ดึง user จาก HttpContext
/// ปรับให้เข้ากับ Auth mechanism จริง (JWT claims ฯลฯ) ได้ภายหลัง
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
