using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ApproveExchangeRate
{
    public class ApproveExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<ApproveExchangeRateCommandHandler> logger)
        : DocumentActionHandlerBase<ApproveExchangeRateCommand>(unitOfWork, documentService, logger)
    {
        protected override Task ExecuteActionAsync(
            ExchangeRateHeader header,
            ApproveExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var docId = header.DocumentId!.Value;
            return ExecuteWithRollbackAsync(
                docId,
                () => _documentService.ApproveAsync(docId, new NotRequireComment { Comment = request.Comment }, cancellationToken),
                "ApproveAsync");
        }
    }
}
