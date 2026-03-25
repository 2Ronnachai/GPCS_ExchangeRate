using AutoMapper;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Domain.Entities;

namespace GPCS_ExchangeRate.Application.Common.Helpers
{
    public static class ExchangeRateHeaderDeltaBuilder
    {
        public static List<ExchangeRateHeaderDeltaDto> Build(
            List<ExchangeRateHeader> headers,
            IMapper mapper)
        {
            var result = new List<ExchangeRateHeaderDeltaDto>();

            for (int i = 0; i < headers.Count; i++)
            {
                var current = headers[i];
                var previous = i < headers.Count - 1 ? headers[i + 1] : null;

                var headerDto = mapper.Map<ExchangeRateHeaderDeltaDto>(current);

                foreach (var detail in headerDto.Details)
                {
                    var prevDetail = previous?.Details
                        .FirstOrDefault(d => d.CurrencyCode == detail.CurrencyCode);

                    (detail.Delta, detail.DeltaPercentage) = ExchangeRateDeltaCalculator
                        .Calculate(detail.Rate, prevDetail?.Rate);
                }

                result.Add(headerDto);
            }

            return result;
        }
    }
}
