using GPCS_ExchangeRate.Api.Models;
using GPCS_ExchangeRate.Application.Features.Dashboards.Dto;
using GPCS_ExchangeRate.Application.Features.Dashboards.Queries.GetDashboard;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ApproveExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CancelExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.RejectExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ReturnExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.SubmitExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetAllExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRateById;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRatesByPeriod;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetRecentExchangeRate;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GPCS_ExchangeRate.Api.Controllers;

public class ExchangeRatesController(IMediator mediator,ICurrentUserService currentUserService) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<ExchangeRateHeaderDetailDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetExchangeRateQuery());
        return OkResponse(result);
    }

    [HttpGet("recent-completed")]
    [ProducesResponseType(typeof(ApiResponse<List<ExchangeRateHeaderDeltaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentCompleted()
    {
        var result = await _mediator.Send(new GetRecentExchangeRateQuery());
        return OkResponse(result);
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard()
    {
        var userName = _currentUserService.GetUserName()
            ?? throw new InvalidOperationException("Current user name cannot be null.");

        var result = await _mediator.Send(new GetDashboardQuery { UserName = userName });
        return OkResponse(result);
    }

    /// <summary>Create a new Exchange Rate document.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExchangeRateHeaderDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExchangeRateCommand command)
    {
        command.UserName = _currentUserService.GetUserName();

        var result = await _mediator.Send(command);
        return CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Exchange rate created successfully.");
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExchangeRateHeaderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateExchangeRateCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id");

        command.UserName = _currentUserService.GetUserName();

        var result = await _mediator.Send(command, ct);

        return result is null
            ? NotFoundResponse($"Exchange rate with ID '{id}' was not found.")
            : OkResponse(result, "Exchange rate updated successfully.");
    }

    /// <summary>Get an Exchange Rate by ID, including its detail lines.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExchangeRateHeaderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetExchangeRateByIdQuery(id));
        return result is null
            ? NotFoundResponse($"Exchange rate with ID '{id}' was not found.")
            : OkResponse(result);
    }

    /// <summary>Get an Exchange Rate by period (yyyyMM format, e.g. 202603).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ExchangeRateHeaderDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPeriod([FromQuery] string period)
    {
        var result = await _mediator.Send(new GetExchangeRatesByPeriodQuery(period));
        return result is null
            ? NotFoundResponse($"Exchange rate for period '{period}' was not found.")
            : OkResponse(result);
    }

    [HttpPost("submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit([FromBody] SubmitExchangeRateCommand command, CancellationToken ct)
    {
        command.UserName = _currentUserService.GetUserName();
        var result = await _mediator.Send(command, ct);
        return result is null
            ? NotFoundResponse($"Exchange rate with ID '{command.Id}' was not found.")
            : OkResponse(result, "Exchange rate submitted successfully.");
    }

    /// <summary>Submit an Exchange Rate document for approval.</summary>
    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(int id, [FromBody] SubmitExchangeRateCommand command, CancellationToken ct)
    {
        command.Id = id;
        command.UserName = _currentUserService.GetUserName();
        var result = await _mediator.Send(command, ct);
        return result is null
            ? NotFoundResponse($"Exchange rate with ID '{command.Id}' was not found.")
            : OkResponse(result, "Exchange rate submitted successfully.");
    }

    /// <summary>Approve an Exchange Rate document.</summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveExchangeRateCommand command, CancellationToken ct)
    {
        command.Id = id;
        var result = await _mediator.Send(command, ct);
        return result is null
            ? NotFoundResponse($"Exchange rate with ID '{command.Id}' was not found.")
            : OkResponse(result, "Exchange rate approved successfully.");
    }

    /// <summary>Reject an Exchange Rate document.</summary>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectExchangeRateCommand command, CancellationToken ct)
    {
        command.Id = id;
        var result = await _mediator.Send(command, ct);
        return result is null
            ? NotFoundResponse($"Exchange rate with ID '{command.Id}' was not found.")
            : OkResponse(result, "Exchange rate rejected successfully.");
    }

    /// <summary>Cancel an Exchange Rate document.</summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelExchangeRateCommand command, CancellationToken ct)
    {
        command.Id = id;
        var result = await _mediator.Send(command, ct);
        return result is null
            ? NotFoundResponse($"Exchange rate with ID '{command.Id}' was not found.")
            : OkResponse(result, "Exchange rate cancelled successfully.");
    }

    /// <summary>Return an Exchange Rate document to the previous step.</summary>
    [HttpPost("{id:int}/return")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Return(int id, [FromBody] ReturnExchangeRateCommand command, CancellationToken ct)
    {
        command.Id = id;
        var result = await _mediator.Send(command, ct);
        return result is null
            ? NotFoundResponse($"Exchange rate with ID '{command.Id}' was not found.")
            : OkResponse(result, "Exchange rate returned successfully.");
    }

    /// <summary>Delete an Exchange Rate and its external document.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteExchangeRateCommand { Id = id }, ct);
        return OkResponse($"Exchange rate with ID '{id}' deleted successfully.");
    }
}
