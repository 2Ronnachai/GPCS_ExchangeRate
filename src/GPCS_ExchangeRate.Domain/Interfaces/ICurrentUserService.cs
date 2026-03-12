namespace GPCS_ExchangeRate.Domain.Interfaces
{
    public interface ICurrentUserService
    {
        /// <summary>Short username only (e.g. "john.doe" from "DOMAIN\john.doe").</summary>
        string? GetUserName();

        /// <summary>Full Windows identity name in DOMAIN\username format (e.g. "CORP\john.doe").</summary>
        string? GetDomainUserName();

        /// <summary>Whether the current HTTP request is authenticated.</summary>
        bool IsAuthenticated { get; }
    }
}
