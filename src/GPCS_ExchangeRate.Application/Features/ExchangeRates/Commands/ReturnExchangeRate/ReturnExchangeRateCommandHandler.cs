using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ReturnExchangeRate
{
    public class ReturnExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<ReturnExchangeRateCommandHandler> logger)
        : IRequestHandler<ReturnExchangeRateCommand>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDocumentService _documentService = documentService;
        private readonly ILogger<ReturnExchangeRateCommandHandler> _logger = logger;

        public async Task Handle(ReturnExchangeRateCommand request, CancellationToken cancellationToken)
        {
            var header = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

            if (header.DocumentId == null)
                throw new InvalidOperationException($"DocumentId is null for ExchangeRateHeader {request.Id}.");

            try
            {
                await _documentService.ReturnAsync(
                    header.DocumentId.Value,
                    new ReturnRequest { Comment = request.Comment },
                    cancellationToken);
            }
            catch
            {
                _logger.LogError("ReturnAsync failed for document {DocumentId}. Attempting RollbackReturnAsync.", header.DocumentId);
                try
                {
                    await _documentService.RollbackReturnAsync(
                        header.DocumentId.Value,
                        new RollbackRequest { Reason = "ReturnAsync failed." },
                        cancellationToken);
                }
                catch
                {
                    _logger.LogError("RollbackReturnAsync also failed for document {DocumentId}.", header.DocumentId);
                }
                throw;
            }
        }
    }
}
