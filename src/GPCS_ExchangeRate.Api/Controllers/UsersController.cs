using GPCS_ExchangeRate.Api.Models;
using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GPCS_ExchangeRate.Api.Controllers
{
    public class UsersController(
        ICurrentUserService currentUserService,
        IUserAccountService userAccountService)
        : ApiControllerBase
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IUserAccountService _userAccountService = userAccountService;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _userAccountService.GetAllAsync(ct);
            return OkResponse(result);
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
        {
            var nId = _currentUserService.GetUserName();
            if (string.IsNullOrEmpty(nId))
                return NotFoundResponse("Current user NID not found.");

            var user = await _userAccountService.GetByNIdAsync(nId, ct);
            return user is null
                ? NotFoundResponse($"User with NID '{nId}' was not found.")
                : OkResponse(user);
        }
    }
}
