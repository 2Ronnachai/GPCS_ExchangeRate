using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;
using GPCS_ExchangeRate.Application.Interfaces.Configurations;
using GPCS_ExchangeRate.Application.Interfaces.External;
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
            {
                // Case 1: First-time submit — Create external document, persist DocumentId, then submit.
                await CreateDocumentAndSubmitAsync(header, request, cancellationToken);
            }
            else
            {
                // Case 2: Re-submit — Update external document, then submit.
                await UpdateDocumentAndSubmitAsync(header, request, cancellationToken);
            }
        }

        private async Task CreateDocumentAndSubmitAsync(
            Domain.Entities.ExchangeRateHeader header,
            SubmitExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var document = new DocumentDto();
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var createDocumentRequest = new CreateDocumentRequest
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

                document = await _documentService.CreateAsync(createDocumentRequest, cancellationToken)
                    ?? throw new InvalidOperationException("External Document API returned null after CreateAsync.");

                header.DocumentId = document.Id;
                header.DocumentNumber = document.DocumentNo;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                if (document?.Id > 0)
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

            // Submit the newly created document
            try
            {
                await _documentService.SubmitAsync(
                    header.DocumentId!.Value,
                    new NotRequireComment { Comment = request.Comment },
                    cancellationToken);
            }
            catch
            {
                _logger.LogError("SubmitAsync failed for document {DocumentId}. Attempting RollbackSubmitAsync.", header.DocumentId);
                try
                {
                    await _documentService.RollbackSubmitAsync(
                        header.DocumentId!.Value,
                        new RollbackRequest { Reason = "SubmitAsync failed during first-time submit." },
                        cancellationToken);
                }
                catch
                {
                    _logger.LogError("RollbackSubmitAsync also failed for document {DocumentId}.", header.DocumentId);
                }
                throw;
            }
        }

        private async Task UpdateDocumentAndSubmitAsync(
            Domain.Entities.ExchangeRateHeader header,
            SubmitExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
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
                    OrganizationalUnitIds = []
                }
            };

            await _documentService.UpdateAsync(
                header.DocumentId!.Value,
                updateDocumentRequest,
                cancellationToken);

            // Submit the updated document
            try
            {
                await _documentService.SubmitAsync(
                    header.DocumentId.Value,
                    new NotRequireComment { Comment = request.Comment },
                    cancellationToken);
            }
            catch
            {
                _logger.LogError("SubmitAsync failed for document {DocumentId}. Attempting RollbackSubmitAsync.", header.DocumentId);
                try
                {
                    await _documentService.RollbackSubmitAsync(
                        header.DocumentId.Value,
                        new RollbackRequest { Reason = "SubmitAsync failed during re-submit." },
                        cancellationToken);
                }
                catch
                {
                    _logger.LogError("RollbackSubmitAsync also failed for document {DocumentId}.", header.DocumentId);
                }
                throw;
            }
        }
    }
}
