using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

namespace GPCS_ExchangeRate.Application.Interfaces.External
{
    public interface IExternalDbService
    {
        Task SyncExchangeRateAsync(
            List<ExchangeRatePayloadDto> payloads,
            CancellationToken cancellationToken);
    }
}
