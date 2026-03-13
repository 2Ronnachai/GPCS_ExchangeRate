using GPCS_ExchangeRate.Application.Common.Helpers;
using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;
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
        IDocumentService documentService,
        ILogger<SubmitExchangeRateCommandHandler> logger,
        IDateTimeService dateTimeService,
        IWorkflowConfiguration workflowConfiguration)
        : IRequestHandler<SubmitExchangeRateCommand>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDocumentService _documentService = documentService;
        private readonly ILogger<SubmitExchangeRateCommandHandler> _logger = logger;
        private readonly IDateTimeService _dateTimeService = dateTimeService;
        private readonly IWorkflowConfiguration _workflowConfiguration = workflowConfiguration;

        public async Task Handle(SubmitExchangeRateCommand request, CancellationToken cancellationToken)
        {
            var header = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

            if (header.DocumentId == null)
                await CreateDocumentAndSubmitAsync(header, request, cancellationToken);
            else
                await UpdateDocumentAndSubmitAsync(header, request, cancellationToken);
        }

        private async Task CreateDocumentAndSubmitAsync(
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

            await SubmitDocumentAsync(document.Id, request.Comment, cancellationToken);
        }

        private async Task UpdateDocumentAndSubmitAsync(
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

            await SubmitDocumentAsync(header.DocumentId!.Value, request.Comment, cancellationToken);
        }

        private async Task SubmitDocumentAsync(int documentId, string? comment, CancellationToken cancellationToken)
        {
            try
            {
                await _documentService.SubmitAsync(
                    documentId,
                    new NotRequireComment { Comment = comment },
                    cancellationToken);

                _logger.LogInformation("Document {DocumentId} submitted successfully.", documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SubmitAsync failed for document {DocumentId}. DB is already saved. User can retry to submit.",
                    documentId);
                throw;
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
