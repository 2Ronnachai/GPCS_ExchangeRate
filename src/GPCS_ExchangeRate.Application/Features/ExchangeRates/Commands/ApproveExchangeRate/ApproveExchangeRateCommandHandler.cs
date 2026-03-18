using AutoMapper;
using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ApproveExchangeRate
{
    public class ApproveExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<ApproveExchangeRateCommandHandler> logger,
        IMapper mapper)
        : DocumentActionHandlerBase<ApproveExchangeRateCommand>(unitOfWork, mapper, documentService, logger)
    {
        protected override Task<ExchangeRateHeaderDetailDto> ExecuteActionAsync(
            ExchangeRateHeader header,
            ApproveExchangeRateCommand request,
            CancellationToken cancellationToken)
            => ExecuteDocumentActionWithStatusUpdateAsync(
                    header,
                    () => _documentService.ApproveAsync(header.DocumentId!.Value, new NotRequireComment { Comment = request.Comment }, cancellationToken),
                    docId => _documentService.RollbackApproveAsync(docId, new RollbackRequest(), cancellationToken),
                    nameof(_documentService.ApproveAsync),
                    cancellationToken);
    }
}
