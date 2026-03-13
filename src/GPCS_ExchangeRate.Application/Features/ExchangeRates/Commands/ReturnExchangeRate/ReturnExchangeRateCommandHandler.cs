using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ReturnExchangeRate
{
    public class ReturnExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<ReturnExchangeRateCommandHandler> logger)
        : DocumentActionHandlerBase<ReturnExchangeRateCommand>(unitOfWork, documentService, logger)
    {
        protected override Task ExecuteActionAsync(
            ExchangeRateHeader header,
            ReturnExchangeRateCommand request,
            CancellationToken cancellationToken)
        {
            var docId = header.DocumentId!.Value;
            return ExecuteWithRollbackAsync(
                docId,
                () => _documentService.ReturnAsync(docId, new ReturnRequest { Comment = request.Comment }, cancellationToken),
                "ReturnAsync");
        }
    }
}
