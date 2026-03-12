using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRateById;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRatesByPeriod;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GPCS_ExchangeRate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExchangeRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Create a new Exchange Rate document.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ExchangeRateHeaderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExchangeRateCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Get an Exchange Rate by ID, including its detail lines.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ExchangeRateHeaderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetExchangeRateByIdQuery(id));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Get an Exchange Rate by period (yyyyMM format, e.g. 202603).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ExchangeRateHeaderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPeriod([FromQuery] string period)
    {
        var result = await _mediator.Send(new GetExchangeRatesByPeriodQuery(period));
        return result is null ? NotFound() : Ok(result);
    }
}
