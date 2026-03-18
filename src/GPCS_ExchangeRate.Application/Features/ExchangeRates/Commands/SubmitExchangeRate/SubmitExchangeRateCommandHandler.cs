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

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.SubmitExchangeRate
{
    public class SubmitExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDocumentService documentService,
        ILogger<SubmitExchangeRateCommandHandler> logger,
        IDateTimeService dateTimeService,
        IWorkflowConfiguration workflowConfiguration)
        : IRequestHandler<SubmitExchangeRateCommand, ExchangeRateHeaderDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IDocumentService _documentService = documentService;
        private readonly ILogger<SubmitExchangeRateCommandHandler> _logger = logger;
        private readonly IDateTimeService _dateTimeService = dateTimeService;
        private readonly IWorkflowConfiguration _workflowConfiguration = workflowConfiguration;

        public async Task<ExchangeRateHeaderDetailDto> Handle(SubmitExchangeRateCommand request, CancellationToken cancellationToken)
        {
            if(request.Id == 0)
            {
                return await CreateAndSubmitAsync(request, cancellationToken);
            }

            var header = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

            if (header.DocumentId == null)
                return await CreateDocumentAndSubmitAsync(header, request, cancellationToken);
            else
                return await UpdateDocumentAndSubmitAsync(header, request, cancellationToken);
        }

        private async Task<ExchangeRateHeaderDetailDto> CreateAndSubmitAsync(
            SubmitExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var periodDate = PeriodParser.Parse(request.Period);
            DocumentDto? document = null;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                document = await _documentService.CreateAsync(
                    BuildCreateDocumentRequest(request), cancellationToken)
                    ?? throw new InvalidOperationException("External Document API returned null after CreateAsync.");
                var header = new ExchangeRateHeader
                {
                    DocumentId = document.Id,
                    DocumentNumber = document.DocumentNo,
                    DocumentStatus = document.DocumentStatus,
                    EffectiveDate = document.EffectiveDate,
                    IsUrgent = document.IsUrgent,
                    Remarks = document.Remarks,
                    CompletedAt = document.CompletedAt
                };
                UpdateExchangeRateData(header, periodDate, request);

                await _unitOfWork.ExchangeRateHeaders.AddAsync(header);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Document {DocumentId} created and DB saved for new ExchangeRateHeader {HeaderId}.",
                    document.Id, header.Id);

                await SubmitDocumentAsync(header, request.Comment, cancellationToken);
                var result = await _unitOfWork.ExchangeRateHeaders
                    .GetWithDetailsAsync(request.Id)
                    ?? header;

                return _mapper.Map<ExchangeRateHeaderDetailDto>(result);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                if (document?.Id > 0)
                    await TryDeleteDocumentAsync(document.Id, cancellationToken);
                throw;
            }
        }

        private async Task<ExchangeRateHeaderDetailDto> CreateDocumentAndSubmitAsync(
            ExchangeRateHeader header,
            SubmitExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var periodDate = PeriodParser.Parse(request.Period);
            DocumentDto? document = null;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                document = await _documentService.CreateAsync(
                    BuildCreateDocumentRequest(request), cancellationToken)
                    ?? throw new InvalidOperationException("External Document API returned null after CreateAsync.");

                header.DocumentId = document.Id;
                header.DocumentNumber = document.DocumentNo;
                header.DocumentStatus = document.DocumentStatus;

                header.EffectiveDate = document.EffectiveDate;
                header.IsUrgent = document.IsUrgent;
                header.Remarks = document.Remarks;

                header.CompletedAt = document.CompletedAt;
                UpdateExchangeRateData(header, periodDate, request);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Document {DocumentId} created and DB saved for ExchangeRateHeader {HeaderId}.",
                    document.Id, header.Id);

            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                if (document?.Id > 0)
                    await TryDeleteDocumentAsync(document.Id, cancellationToken);

                throw;
            }

            await SubmitDocumentAsync(header, request.Comment, cancellationToken);
            var result = await _unitOfWork.ExchangeRateHeaders
                    .GetWithDetailsAsync(request.Id)
                    ?? header;

            return _mapper.Map<ExchangeRateHeaderDetailDto>(result);
        }

        private async Task<ExchangeRateHeaderDetailDto> UpdateDocumentAndSubmitAsync(
            ExchangeRateHeader header,
            SubmitExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var periodDate = PeriodParser.Parse(request.Period);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _documentService.UpdateAsync(
                    header.DocumentId!.Value,
                    BuildUpdateDocumentRequest(request),
                    cancellationToken);

                UpdateExchangeRateData(header, periodDate, request);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Document {DocumentId} updated and DB saved for ExchangeRateHeader {HeaderId}.",
                    header.DocumentId.Value, header.Id);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            await SubmitDocumentAsync(header, request.Comment, cancellationToken);
            var result = await _unitOfWork.ExchangeRateHeaders
                    .GetWithDetailsAsync(request.Id)
                    ?? header;

            return _mapper.Map<ExchangeRateHeaderDetailDto>(result);
        }

        private async Task SubmitDocumentAsync(
            ExchangeRateHeader header,
            string? comment,
            CancellationToken cancellationToken)
        {
            var docId = header.DocumentId!.Value;

            var document = await _documentService.SubmitAsync(
                docId,
                new NotRequireComment { Comment = comment },
                cancellationToken)
                ?? throw new InvalidOperationException(
                    $"External Document API returned null after SubmitAsync for document {docId}.");

            _logger.LogInformation("Document {DocumentId} submitted successfully.", docId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                header.DocumentStatus = document.DocumentStatus;
                header.CompletedAt = document.CompletedAt;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "DocumentStatus updated to '{Status}' for ExchangeRateHeader {HeaderId}.",
                    document.DocumentStatus, header.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex,
                    "Failed to save DocumentStatus after SubmitAsync for document {DocumentId}. Attempting RollbackSubmitAsync.",
                    docId);

                await TryRollbackSubmitAsync(docId, cancellationToken);

                throw;
            }
        }

        private async Task TryRollbackSubmitAsync(int documentId, CancellationToken cancellationToken)
        {
            try
            {
                await _documentService.RollbackSubmitAsync(
                    documentId,
                    new RollbackRequest(),
                    cancellationToken);

                _logger.LogInformation(
                    "Compensation: RollbackSubmitAsync succeeded for document {DocumentId}.",
                    documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Compensation failed: RollbackSubmitAsync failed for document {DocumentId}. Manual cleanup required.",
                    documentId);
            }
        }

        private async Task TryDeleteDocumentAsync(int documentId, CancellationToken cancellationToken)
        {
            try
            {
                await _documentService.DeleteAsync(documentId, cancellationToken);
                _logger.LogInformation("Compensation: Deleted document {DocumentId}.", documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Compensation failed: Unable to delete document {DocumentId}. Manual cleanup required.",
                    documentId);
            }
        }

        private static void UpdateExchangeRateData(
            ExchangeRateHeader header,
            DateTime periodDate,
            SubmitExchangeRateCommand request)
        {
            header.Period = periodDate;
            header.Details = request.Items.Select(item => new ExchangeRateDetail
            {
                CurrencyCode = item.CurrencyCode.ToUpperInvariant(),
                Rate = item.Rate,
                Rate2Digit = Math.Round(item.Rate, 2, MidpointRounding.AwayFromZero),
                Rate4Digit = Math.Round(item.Rate, 4, MidpointRounding.AwayFromZero),
            }).ToList();
        }

        private CreateDocumentRequest BuildCreateDocumentRequest(SubmitExchangeRateCommand request) =>
            new()
            {
                Title = request.Title,
                DocumentType = _workflowConfiguration.DocumentType,
                EffectiveDate = request.EffectiveDate ?? _dateTimeService.Now,
                IsUrgent = request.IsUrgent,
                Remarks = request.Remarks,
                Comment = request.Comment,
                WorkflowCreateRequest = new CreateWorkflowInstanceRequest
                {
                    RouteId = _workflowConfiguration.RouteId,
                    NIdRequester = request.UserName!,
                    OrganizationalUnitIds = []
                }
            };

        private UpdateDocumentRequest BuildUpdateDocumentRequest(SubmitExchangeRateCommand request) =>
            new()
            {
                Title = request.Title,
                EffectiveDate = request.EffectiveDate ?? _dateTimeService.Now,
                IsUrgent = request.IsUrgent,
                Remarks = request.Remarks,
                Comment = request.Comment,
                WorkflowUpdateRequest = new UpdateWorkflowInstanceRequest
                {
                    RouteId = _workflowConfiguration.RouteId,
                    NIdRequester = request.UserName!,
                    OrganizationalUnitIds = []
                }
            };

    }
}
