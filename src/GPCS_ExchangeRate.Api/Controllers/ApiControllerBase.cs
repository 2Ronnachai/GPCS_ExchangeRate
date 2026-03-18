using GPCS_ExchangeRate.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace GPCS_ExchangeRate.Api.Controllers;

/// <summary>
/// Base controller that wraps every response inside <see cref="ApiResponse{T}"/>.
/// All API controllers should inherit from this class instead of <see cref="ControllerBase"/>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkResponse(string message = "")
        => Ok(ApiResponse.Ok(message));

    protected IActionResult OkResponse<T>(T data, string message = "")
        => Ok(ApiResponse<T>.Ok(data, message));

    protected IActionResult CreatedResponse<T>(string actionName, object routeValues, T data, string message = "")
        => CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(data, message));

    protected IActionResult NotFoundResponse(string message = "Resource not found.")
        => NotFound(ApiResponse.Fail(message, errorCode: "NOT_FOUND"));

    protected IActionResult BadRequestResponse(string message, Dictionary<string, string[]>? errors = null)
        => BadRequest(ApiResponse.Fail(message, errorCode: "BAD_REQUEST", errors: errors));

    protected IActionResult NoContentResponse()
        => NoContent();
}
