using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;

namespace GPCS_ExchangeRate.Application.Interfaces.External
{
    public interface IUserAccountService
    {
        Task<UserDto?> GetByNIdAsync(string nId, CancellationToken cancellationToken = default);
        Task<List<UserDto>> GetByNIdsAsync(List<string> nIds, CancellationToken cancellationToken = default);
        Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
