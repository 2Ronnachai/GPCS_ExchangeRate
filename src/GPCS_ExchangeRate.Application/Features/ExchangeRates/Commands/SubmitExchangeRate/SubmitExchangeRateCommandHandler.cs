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

            var isDuplicate = await _unitOfWork.ExchangeRateHeaders
                .ExistByPeriodAsync(periodDate, cancellationToken);

            if (isDuplicate)
                throw new ArgumentException($"Exchange rate for period '{request.Period}' already exists.");

            DocumentDto? document = null;
            DocumentDto? submittedDocument = null;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var header = new ExchangeRateHeader();
                UpdateExchangeRateData(header, periodDate, request);

                await _unitOfWork.ExchangeRateHeaders.AddAsync(header);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                document = await _documentService.CreateAsync(
                    BuildCreateDocumentRequest(request, header.Id), cancellationToken)
                    ?? throw new InvalidOperationException(
                        "External Document API returned null after CreateAsync.");

                submittedDocument = await _documentService.SubmitAsync(
                    document.Id,
                    new NotRequireComment { Comment = request.Comment },
                    cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"External Document API returned null after SubmitAsync for document {document.Id}.");

                header.DocumentId = submittedDocument.Id;
                header.DocumentNumber = submittedDocument.DocumentNo;
                header.DocumentStatus = submittedDocument.DocumentStatus;
                header.EffectiveDate = submittedDocument.EffectiveDate;
                header.IsUrgent = submittedDocument.IsUrgent;
                header.Remarks = submittedDocument.Remarks;
                header.CompletedAt = submittedDocument.CompletedAt;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Document {DocumentId} created and submitted for ExchangeRateHeader {HeaderId}.",
                    submittedDocument.Id, header.Id);

                var result = await _unitOfWork.ExchangeRateHeaders
                    .GetWithDetailsAsync(header.Id) ?? header;

                return _mapper.Map<ExchangeRateHeaderDetailDto>(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex, "Failed to create and submit document.");

                // Compensate External API
                if (submittedDocument?.Id > 0)
                    await TryRollbackSubmitAsync(submittedDocument.Id, cancellationToken);
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

            var isDuplicate = await _unitOfWork.ExchangeRateHeaders
                .ExistByPeriodAsync(periodDate, cancellationToken);

            if (isDuplicate)
                throw new ArgumentException($"Exchange rate for period '{request.Period}' already exists.");

            DocumentDto? document = null;
            DocumentDto? submittedDocument = null;

            try
            {
                document = await _documentService.CreateAsync(
                   BuildCreateDocumentRequest(request, header.Id), cancellationToken)
                   ?? throw new InvalidOperationException(
                       "External Document API returned null after CreateAsync.");

                submittedDocument = await _documentService.SubmitAsync(
                    document.Id,
                    new NotRequireComment { Comment = request.Comment },
                    cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"External Document API returned null after SubmitAsync for document {document.Id}.");

                _logger.LogInformation(
                    "Document {DocumentId} created and submitted successfully.", document.Id);
            }
            catch
            {
                if (document?.Id > 0)
                    await TryDeleteDocumentAsync(document.Id, cancellationToken);
                throw;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                header.DocumentId = submittedDocument.Id;
                header.DocumentNumber = submittedDocument.DocumentNo;
                header.DocumentStatus = submittedDocument.DocumentStatus;
                header.EffectiveDate = submittedDocument.EffectiveDate;
                header.IsUrgent = submittedDocument.IsUrgent;
                header.Remarks = submittedDocument.Remarks;
                header.CompletedAt = submittedDocument.CompletedAt;
                UpdateExchangeRateData(header, periodDate, request);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "DB saved for ExchangeRateHeader {HeaderId} with Document {DocumentId}.",
                    header.Id, submittedDocument.Id);

                var result = await _unitOfWork.ExchangeRateHeaders
                    .GetWithDetailsAsync(header.Id) ?? header;

                return _mapper.Map<ExchangeRateHeaderDetailDto>(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex,
                    "DB save failed after Create+Submit for document {DocumentId}. " +
                    "Attempting RollbackSubmit then Delete.",
                    submittedDocument.Id);

                await TryRollbackSubmitAsync(submittedDocument.Id, cancellationToken);
                await TryDeleteDocumentAsync(submittedDocument.Id, cancellationToken);
                throw;
            }
        }

        private async Task<ExchangeRateHeaderDetailDto> UpdateDocumentAndSubmitAsync(
            ExchangeRateHeader header,
            SubmitExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var periodDate = PeriodParser.Parse(request.Period);

            DocumentDto? submittedDocument = null;
            try
            {
                await _documentService.UpdateAsync(
                    header.DocumentId!.Value,
                    BuildUpdateDocumentRequest(request, header.DocumentId.Value),
                    cancellationToken);

                submittedDocument = await _documentService.SubmitAsync(
                    header.DocumentId!.Value,
                    new NotRequireComment { Comment = request.Comment },
                    cancellationToken)
                    ?? throw new InvalidOperationException(
                        $"External Document API returned null after SubmitAsync for document {header.DocumentId.Value}.");

                _logger.LogInformation(
                    "Document {DocumentId} updated and submitted successfully.", header.DocumentId.Value);
            }
            catch
            {
                throw;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                UpdateExchangeRateData(header, periodDate, request);
                header.DocumentStatus = submittedDocument.DocumentStatus;
                header.CompletedAt = submittedDocument.CompletedAt;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "DB saved for ExchangeRateHeader {HeaderId} with Document {DocumentId}.",
                    header.Id, header.DocumentId!.Value);

                var result = await _unitOfWork.ExchangeRateHeaders
                    .GetWithDetailsAsync(request.Id) ?? header;

                return _mapper.Map<ExchangeRateHeaderDetailDto>(result);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex,
                    "DB save failed after Update+Submit for document {DocumentId}. " +
                    "Attempting RollbackSubmit.",
                    header.DocumentId!.Value);

                await TryRollbackSubmitAsync(header.DocumentId!.Value, cancellationToken);
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

        private CreateDocumentRequest BuildCreateDocumentRequest(SubmitExchangeRateCommand request, int headerId) =>
            new()
            {
                Title = request.Title,
                DocumentType = _workflowConfiguration.DocumentType,
                EffectiveDate = request.EffectiveDate ?? _dateTimeService.Now,
                IsUrgent = request.IsUrgent,
                Remarks = request.Remarks,
                Comment = request.Comment,
                SourceDocumentId = headerId.ToString(),
                // SourceUrl = External API will be calculated based on DocumentId, so no need to set here
                WorkflowCreateRequest = new CreateWorkflowInstanceRequest
                {
                    RouteId = _workflowConfiguration.RouteId,
                    NIdRequester = request.UserName!,
                    OrganizationalUnitIds = []
                }
            };

        private UpdateDocumentRequest BuildUpdateDocumentRequest(SubmitExchangeRateCommand request, int documentId) =>
            new()
            {
                DocumentId = documentId,
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
