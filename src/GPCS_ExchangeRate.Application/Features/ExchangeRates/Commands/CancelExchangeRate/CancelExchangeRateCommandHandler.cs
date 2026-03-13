using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CancelExchangeRate
{
    public class CancelExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<CancelExchangeRateCommandHandler> logger)
        : DocumentActionHandlerBase<CancelExchangeRateCommand>(unitOfWork, documentService, logger)
    {
        protected override Task ExecuteActionAsync(
            ExchangeRateHeader header,
            CancelExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var docId = header.DocumentId!.Value;
            return ExecuteWithRollbackAsync(
                docId,
                () => _documentService.CancelAsync(docId, new RequireComment { Comment = request.Comment }, cancellationToken),
                "CancelAsync");
        }
    }
}
