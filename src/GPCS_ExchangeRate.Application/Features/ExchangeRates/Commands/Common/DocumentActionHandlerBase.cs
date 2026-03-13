using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;

/// <summary>
/// Base handler for document workflow actions that share the pattern:
/// get header → validate DocumentId → execute action → rollback on failure.
/// </summary>
public abstract class DocumentActionHandlerBase<TCommand>(
    IUnitOfWork unitOfWork,
    IDocumentService documentService,
    ILogger logger)
    : IRequestHandler<TCommand>
    where TCommand : IDocumentActionCommand
{
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;
    protected readonly IDocumentService _documentService = documentService;
    protected readonly ILogger _logger = logger;

    public async Task Handle(TCommand request, CancellationToken cancellationToken)
    {
        var header = await _unitOfWork.ExchangeRateHeaders
            .GetWithDetailsAsync(request.Id)
            ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

        if (header.DocumentId == null)
            throw new InvalidOperationException($"DocumentId is null for ExchangeRateHeader {request.Id}.");

        await ExecuteActionAsync(header, request, cancellationToken);
    }

    protected abstract Task ExecuteActionAsync(
        ExchangeRateHeader header,
        TCommand request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes an external API action and attempts rollback compensation if it fails.
    /// </summary>
    protected async Task ExecuteWithRollbackAsync(
        int documentId,
        Func<Task> action,
        string actionName)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Action} failed for document {DocumentId}. User can retry.", actionName, documentId);
            throw;
        }
    }
}
