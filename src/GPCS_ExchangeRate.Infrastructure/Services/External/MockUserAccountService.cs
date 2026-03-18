using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace GPCS_ExchangeRate.Infrastructure.Services.External
{
    public class MockUserAccountService : IUserAccountService
    {
        private readonly List<MockUserDto> _users;

        public MockUserAccountService(IOptions<MockUserOptions> options)
        {
            _users = options.Value.Users;
        }

        public Task<UserDto?> GetByNIdAsync(string nId, CancellationToken cancellationToken = default)
        {
            var user = _users.FirstOrDefault(u => u.NId == nId);
            return Task.FromResult(user is null ? null : MapToDto(user));
        }

        public Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = _users.Select(MapToDto).ToList();
            return Task.FromResult(result);
        }

        private static UserDto MapToDto(MockUserDto mock) => new()
        {
            NId = mock.NId,
            FullName = mock.FullName,
            Email = mock.Email,
            Department = mock.Department,
            Roles = mock.Roles
        };
    }
}
