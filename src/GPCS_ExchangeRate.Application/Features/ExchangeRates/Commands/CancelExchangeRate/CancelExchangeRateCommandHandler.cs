using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CancelExchangeRate
{
    public class CancelExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<CancelExchangeRateCommandHandler> logger)
        : IRequestHandler<CancelExchangeRateCommand>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDocumentService _documentService = documentService;
        private readonly ILogger<CancelExchangeRateCommandHandler> _logger = logger;

        public async Task Handle(CancelExchangeRateCommand request, CancellationToken cancellationToken)
        {
            var header = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

            if (header.DocumentId == null)
                throw new InvalidOperationException($"DocumentId is null for ExchangeRateHeader {request.Id}.");

            try
            {
                await _documentService.CancelAsync(
                    header.DocumentId.Value,
                    new RequireComment { Comment = request.Comment },
                    cancellationToken);
            }
            catch
            {
                _logger.LogError("CancelAsync failed for document {DocumentId}. Attempting RollbackCancelAsync.", header.DocumentId);
                try
                {
                    await _documentService.RollbackCancelAsync(
                        header.DocumentId.Value,
                        new RollbackRequest { Reason = "CancelAsync failed." },
                        cancellationToken);
                }
                catch
                {
                    _logger.LogError("RollbackCancelAsync also failed for document {DocumentId}.", header.DocumentId);
                }
                throw;
            }
        }
    }
}
