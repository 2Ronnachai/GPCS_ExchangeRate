using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GPCS_ExchangeRate.Infrastructure.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private string? RawIdentityName
            => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        /// <inheritdoc/>
        /// <remarks>
        /// Returns the short username only (e.g. "john.doe") extracted from
        /// the Windows identity "DOMAIN\username" format.
        /// </remarks>
        public string? GetUserName()
        {
            var raw = RawIdentityName;
            if (string.IsNullOrEmpty(raw))
                return null;

            // Handle both DOMAIN\username (NTLM/Negotiate) and UPN user@domain formats
            if (raw.Contains('\\'))
                return raw.Split('\\', 2)[1];

            if (raw.Contains('@'))
                return raw.Split('@', 2)[0];

            return raw;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Returns the full Windows identity name in DOMAIN\username format (e.g. "CORP\john.doe").
        /// </remarks>
        public string? GetDomainUserName()
        {
            var raw = RawIdentityName;
            return string.IsNullOrEmpty(raw) ? null : raw;
        }

        /// <inheritdoc/>
        public bool IsAuthenticated
            => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
