using AutoMapper;
using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ReturnExchangeRate
{
    public class ReturnExchangeRateCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<ReturnExchangeRateCommandHandler> logger)
        : DocumentActionHandlerBase<ReturnExchangeRateCommand>(unitOfWork, mapper, documentService, logger)
    {
        protected override Task<ExchangeRateHeaderDetailDto> ExecuteActionAsync(
           ExchangeRateHeader header,
           ReturnExchangeRateCommand request,
           CancellationToken cancellationToken)
           => ExecuteDocumentActionWithStatusUpdateAsync(
               header,
               () => _documentService.ReturnAsync(header.DocumentId!.Value, new ReturnRequest { Comment = request.Comment }, cancellationToken),
               docId => _documentService.RollbackReturnAsync(docId, new RollbackRequest(), cancellationToken),
               nameof(_documentService.ReturnAsync),
               cancellationToken);
    }
}
