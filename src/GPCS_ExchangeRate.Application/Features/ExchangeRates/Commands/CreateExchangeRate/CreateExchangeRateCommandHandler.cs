using AutoMapper;
using GPCS_ExchangeRate.Application.Common.Helpers;
using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.Configurations;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IDateTimeService dateTimeService,
    IDocumentService documentService,
    ILogger<CreateExchangeRateCommandHandler> logger,
    IWorkflowConfiguration workflowConfiguration) :
    IRequestHandler<CreateExchangeRateCommand, ExchangeRateHeaderDetailDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IDateTimeService _dateTimeService = dateTimeService;
    private readonly IDocumentService _documentService = documentService;
    private readonly ILogger<CreateExchangeRateCommandHandler> _logger = logger;
    private readonly IWorkflowConfiguration _workflowConfiguration = workflowConfiguration;

    public async Task<ExchangeRateHeaderDetailDto> Handle(
        CreateExchangeRateCommand request,
        CancellationToken cancellationToken)
    {
        var periodDate = PeriodParser.Parse(request.Period);

        var isDuplicate = await _unitOfWork.ExchangeRateHeaders
            .ExistByPeriodAsync(periodDate, cancellationToken);

        if (isDuplicate)
            throw new ArgumentException($"Exchange rate for period '{request.Period}' already exists.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        var document = new DocumentDto();
        try
        {
            var createDocumentRequest = new CreateDocumentRequest
            {
                Title = request.Title,
                DocumentType = _workflowConfiguration.DocumentType,
                EffectiveDate = request.EffectiveDate ?? _dateTimeService.Now,
                IsUrgent = request.IsUrgent,
                Remarks = request.Remarks ?? string.Empty,
                Comment = request.Comment,
                WorkflowCreateRequest = new CreateWorkflowInstanceRequest
                {
                    RouteId = _workflowConfiguration.RouteId,
                    NIdRequester = request.UserName!,
                    OrganizationalUnitIds = []
                }
            };

            document = await _documentService.CreateAsync(createDocumentRequest, cancellationToken);

            if (document == null)
                throw new Exception("Failed to create document.");

            var header = new ExchangeRateHeader
            {
                Period = periodDate,

                // Link to the created document
                DocumentId = document.Id,
                DocumentNumber = document.DocumentNo,
                DocumentStatus = document.DocumentStatus,
                
                IsUrgent = document.IsUrgent,
                EffectiveDate = document.EffectiveDate,
                Remarks = document.Remarks,

                Details = request.Items.Select(item => new ExchangeRateDetail
                {
                    CurrencyCode = item.CurrencyCode.ToUpperInvariant(),
                    Rate = item.Rate,
                    Rate2Digit = Math.Round(item.Rate, 2, MidpointRounding.AwayFromZero),
                    Rate4Digit = Math.Round(item.Rate, 4, MidpointRounding.AwayFromZero),
                }).ToList()
            };

            await _unitOfWork.ExchangeRateHeaders.AddAsync(header);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var result = await _unitOfWork.ExchangeRateHeaders.GetWithDetailsAsync(header.Id);

            return _mapper.Map<ExchangeRateHeaderDetailDto>(result);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            // Compensation
            if (document != null)
            {
                try
                {
                    await _documentService.DeleteAsync(document.Id, cancellationToken);
                }
                catch
                {
                    _logger.LogError("Failed to delete document with ID {DocumentId} during compensation.", document.Id);
                }
            }
            throw;
        }
    }
}
