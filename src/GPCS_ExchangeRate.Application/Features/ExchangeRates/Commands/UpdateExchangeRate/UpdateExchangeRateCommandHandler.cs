using AutoMapper;
using GPCS_ExchangeRate.Application.Common.Helpers;
using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.Configurations;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.UpdateExchangeRate
{
    public class UpdateExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDateTimeService dateTimeService,
        IDocumentService documentService,
        IWorkflowConfiguration workflowConfiguration)
        : IRequestHandler<UpdateExchangeRateCommand, ExchangeRateHeaderDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IDateTimeService _dateTimeService = dateTimeService;
        private readonly IDocumentService _documentService = documentService;
        private readonly IWorkflowConfiguration _workflowConfiguration = workflowConfiguration;

        public async Task<ExchangeRateHeaderDto> Handle(
            UpdateExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var periodDate = PeriodParser.Parse(request.Period);

            var header = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(request.Id) 
                ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

            if (header.DocumentId == null)
                throw new InvalidOperationException($"DocumentId is null for ExchangeRateHeader {request.Id}.");

            var isDuplicate = await _unitOfWork.ExchangeRateHeaders
                .AnyAsync(x => x.Period == periodDate && x.Id != header.Id, cancellationToken);

            if (isDuplicate)
                throw new ArgumentException($"Exchange rate for period '{request.Period}' already exists.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1 Update Document
                var updateDocumentRequest = new UpdateDocumentRequest
                {
                    Title = request.Title,
                    EffectiveDate = request.EffectiveDate ?? _dateTimeService.Now,
                    IsUrgent = request.IsUrgent,
                    Remarks = request.Remarks,
                    Comment = request.Comment,
                    WorkflowUpdateRequest = new UpdateWorkflowInstanceRequest
                    {
                        RouteId = _workflowConfiguration.RouteId,
                        NIdRequester = request.UserName ?? "system",
                        OrganizationalUnitIds = [],
                        // Skip OrganizationalUnitCodes = null
                        // Skip ConditionalData = null
                    }
                };

                await _documentService.UpdateAsync(
                    header.DocumentId.Value,
                    updateDocumentRequest,
                    cancellationToken);

                // 2 Update ExchangeRateHeader
                header.Period = periodDate;

                // 3 Update ExchangeRateDetails
                header.Details = request.Items.Select(item => new Domain.Entities.ExchangeRateDetail
                {
                    CurrencyCode = item.CurrencyCode.ToUpperInvariant(),
                    Rate = item.Rate,
                    Rate2Digit = Math.Round(item.Rate, 2, MidpointRounding.AwayFromZero),
                    Rate4Digit = Math.Round(item.Rate, 4, MidpointRounding.AwayFromZero),
                }).ToList();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var result = await _unitOfWork.ExchangeRateHeaders
                    .GetWithDetailsAsync(header.Id);

                return _mapper.Map<ExchangeRateHeaderDto>(result);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
