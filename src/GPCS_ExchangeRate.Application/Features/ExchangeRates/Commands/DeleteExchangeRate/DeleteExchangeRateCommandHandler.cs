using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.DeleteExchangeRate
{
    public class DeleteExchangeRateCommandHandler(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        ILogger<DeleteExchangeRateCommandHandler> logger)
        : IRequestHandler<DeleteExchangeRateCommand>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDocumentService _documentService = documentService;
        private readonly ILogger<DeleteExchangeRateCommandHandler> _logger = logger;

        public async Task Handle(DeleteExchangeRateCommand request, CancellationToken cancellationToken)
        {
            var header = await _unitOfWork.ExchangeRateHeaders
                .GetWithDetailsAsync(request.Id)
                ?? throw new KeyNotFoundException($"ExchangeRateHeader {request.Id} not found.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // Delete external document first (if exists)
                if (header.DocumentId != null)
                {
                    await _documentService.DeleteAsync(header.DocumentId.Value, cancellationToken);
                }

                _unitOfWork.ExchangeRateHeaders.Delete(header);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError("Failed to delete ExchangeRateHeader {HeaderId}.", request.Id);
                throw;
            }
        }
    }
}
