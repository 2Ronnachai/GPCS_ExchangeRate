using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.ApproveExchangeRate
{
    public class ApproveExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<ApproveExchangeRateCommandHandler> logger)
        : IRequestHandler<ApproveExchangeRateCommand>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDocumentService _documentService = documentService;
        private readonly ILogger<ApproveExchangeRateCommandHandler> _logger = logger;

        public async Task Handle(ApproveExchangeRateCommand request, CancellationToken cancellationToken)
        {
            var header = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

            if (header.DocumentId == null)
                throw new InvalidOperationException($"DocumentId is null for ExchangeRateHeader {request.Id}.");

            try
            {
                await _documentService.ApproveAsync(
                    header.DocumentId.Value,
                    new NotRequireComment { Comment = request.Comment },
                    cancellationToken);
            }
            catch
            {
                _logger.LogError("ApproveAsync failed for document {DocumentId}. Attempting RollbackApproveAsync.", header.DocumentId);
                try
                {
                    await _documentService.RollbackApproveAsync(
                        header.DocumentId.Value,
                        new RollbackRequest { Reason = "ApproveAsync failed." },
                        cancellationToken);
                }
                catch
                {
                    _logger.LogError("RollbackApproveAsync also failed for document {DocumentId}.", header.DocumentId);
                }
                throw;
            }
        }
    }
}
