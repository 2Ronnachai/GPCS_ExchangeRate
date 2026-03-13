using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.RejectExchangeRate
{
    public class RejectExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<RejectExchangeRateCommandHandler> logger)
        : IRequestHandler<RejectExchangeRateCommand>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDocumentService _documentService = documentService;
        private readonly ILogger<RejectExchangeRateCommandHandler> _logger = logger;

        public async Task Handle(RejectExchangeRateCommand request, CancellationToken cancellationToken)
        {
            var header = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

            if (header.DocumentId == null)
                throw new InvalidOperationException($"DocumentId is null for ExchangeRateHeader {request.Id}.");

            try
            {
                await _documentService.RejectAsync(
                    header.DocumentId.Value,
                    new RequireComment { Comment = request.Comment },
                    cancellationToken);
            }
            catch
            {
                _logger.LogError("RejectAsync failed for document {DocumentId}. Attempting RollbackRejectAsync.", header.DocumentId);
                try
                {
                    await _documentService.RollbackRejectAsync(
                        header.DocumentId.Value,
                        new RollbackRequest { Reason = "RejectAsync failed." },
                        cancellationToken);
                }
                catch
                {
                    _logger.LogError("RollbackRejectAsync also failed for document {DocumentId}.", header.DocumentId);
                }
                throw;
            }
        }
    }
}
