using AutoMapper;
using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;

/// <summary>
/// Base handler for document workflow actions that share the pattern:
/// get header → validate DocumentId → execute action → rollback on failure.
/// </summary>
public abstract class DocumentActionHandlerBase<TCommand>(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IDocumentService documentService,
    ILogger logger)
    : IRequestHandler<TCommand, ExchangeRateHeaderDetailDto>
    where TCommand : IDocumentActionCommand
{
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;
    protected readonly IMapper _mapper = mapper;
    protected readonly IDocumentService _documentService = documentService;
    protected readonly ILogger _logger = logger;

    public async Task<ExchangeRateHeaderDetailDto> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var header = await _unitOfWork.ExchangeRateHeaders
            .GetWithDetailsAsync(request.Id)
            ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

        if (header.DocumentId == null)
            throw new InvalidOperationException($"DocumentId is null for ExchangeRateHeader {request.Id}.");

        return await ExecuteActionAsync(header, request, cancellationToken);
    }

    protected abstract Task<ExchangeRateHeaderDetailDto> ExecuteActionAsync(
        ExchangeRateHeader header,
        TCommand request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes an external API action and attempts rollback compensation if it fails.
    /// </summary>
    protected async Task<ExchangeRateHeaderDetailDto> ExecuteDocumentActionWithStatusUpdateAsync(
          ExchangeRateHeader header,
          Func<Task<DocumentDto?>> action,
          Func<int, Task> rollbackAction,
          string actionName,
          CancellationToken cancellationToken)
    {
        var docId = header.DocumentId!.Value;

        var document = await action()
            ?? throw new InvalidOperationException(
                $"External Document API returned null after {actionName} for document {docId}.");

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            header.DocumentStatus = document.DocumentStatus;

            header.EffectiveDate = document.EffectiveDate;
            header.IsUrgent = document.IsUrgent;
            header.Remarks = document.Remarks;

            header.CompletedAt = document.CompletedAt;

            await AddToOutboxAsync(header, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "{ActionName} succeeded for document {DocumentId}. Status updated to '{Status}' for ExchangeRateHeader {HeaderId}.",
                actionName, docId, document.DocumentStatus, header.Id);

            var result = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(header.Id)
                ?? header;

            return _mapper.Map<ExchangeRateHeaderDetailDto>(result);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(ex,
                "Failed to save DocumentStatus after {ActionName} for document {DocumentId}. Attempting rollback.",
                actionName, docId);

            await TryRollbackAsync(docId, rollbackAction, actionName);

            throw;
        }
    }

    private async Task AddToOutboxAsync(ExchangeRateHeader header, CancellationToken cancellationToken)
    {
        if(header.DocumentStatus == "Completed" && header.CompletedAt != null)
        {
            var payloads = new List<ExchangeRatePayloadDto>();
            foreach (var detail in header.Details)
            {
                payloads.Add(new ExchangeRatePayloadDto
                {
                    CurrencyCode = detail.CurrencyCode,
                    Period = header.Period.ToString("yyyyMM"),
                    Rate = detail.Rate4Digit,
                    Rate2 = detail.Rate,
                    AppUserID = header.CreatedBy ?? "System",
                    AppDate = header.CreatedAt.ToString("yyyyMMdd"),
                    UpdUserID = header.UpdatedBy,
                    UpdDate = header.UpdatedAt?.ToString("yyyyMMdd"),
                    UpdPGM = string.Empty
                });
            }

            var outboxEntry = new ExchangeRateOutBoxEvents
            {
                EventType = "ExchangeRate",
                Payload = JsonSerializer.Serialize(payloads),
            };

            await _unitOfWork.ExchangeRateOutBoxEvents.AddAsync(outboxEntry);

            _logger.LogInformation(
                    "Outbox entry created for ExchangeRateHeader {HeaderId} with DocumentStatus 'Completed'. Payload: {Payload}",
                    header.Id, outboxEntry.Payload);
        }
    }

    private async Task TryRollbackAsync(
        int documentId,
        Func<int, Task> rollbackAction,
        string actionName)
    {
        try
        {
            await rollbackAction(documentId);

            _logger.LogInformation(
                "Compensation: Rollback{ActionName} succeeded for document {DocumentId}.",
                actionName, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Compensation failed: Rollback{ActionName} failed for document {DocumentId}. Manual cleanup required.",
                actionName, documentId);
        }
    }
}
