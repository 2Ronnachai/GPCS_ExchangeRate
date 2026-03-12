using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRateById;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRatesByPeriod;
using GPCS_ExchangeRate.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using GPCS_ExchangeRate.Application.Interfaces.External;

namespace GPCS_ExchangeRate.Api.Controllers;

public class ExchangeRatesController(IMediator mediator,
    IDocumentService documentService) : ApiControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IDocumentService _documentService = documentService;

    [HttpGet("document/{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentUrl(int id)
    {
        var documentUrl = await _documentService.GetByIdAsync(id);
        return documentUrl is null
            ? NotFoundResponse($"Document with ID '{id}' was not found.")
            : OkResponse(documentUrl, "Document URL retrieved successfully.");
    }

    /// <summary>Create a new Exchange Rate document.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExchangeRateHeaderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExchangeRateCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedResponse(nameof(GetById), new { id = result.Id }, result, "Exchange rate created successfully.");
    }

    /// <summary>Get an Exchange Rate by ID, including its detail lines.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ExchangeRateHeaderDto>), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(ApiResponse<ExchangeRateHeaderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPeriod([FromQuery] string period)
    {
        var result = await _mediator.Send(new GetExchangeRatesByPeriodQuery(period));
        return result is null
            ? NotFoundResponse($"Exchange rate for period '{period}' was not found.")
            : OkResponse(result);
    }
}
