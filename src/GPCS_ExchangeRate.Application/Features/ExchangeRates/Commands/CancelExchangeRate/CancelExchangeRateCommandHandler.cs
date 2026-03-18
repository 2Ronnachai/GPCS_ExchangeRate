using AutoMapper;
using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CancelExchangeRate
{
    public class CancelExchangeRateCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<CancelExchangeRateCommandHandler> logger)
        : DocumentActionHandlerBase<CancelExchangeRateCommand>(unitOfWork, mapper, documentService, logger)
    {
        protected override Task<ExchangeRateHeaderDetailDto> ExecuteActionAsync(
            ExchangeRateHeader header,
            CancelExchangeRateCommand request,
            CancellationToken cancellationToken)
            => ExecuteDocumentActionWithStatusUpdateAsync(
                header,
                () => _documentService.CancelAsync(header.DocumentId!.Value, new RequireComment { Comment = request.Comment }, cancellationToken),
                docId => _documentService.RollbackCancelAsync(docId, new RollbackRequest(), cancellationToken),
                nameof(_documentService.CancelAsync),
                cancellationToken);
    }
}
