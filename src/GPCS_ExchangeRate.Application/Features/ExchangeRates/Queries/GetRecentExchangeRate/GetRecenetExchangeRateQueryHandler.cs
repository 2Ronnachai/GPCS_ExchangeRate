using AutoMapper;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetRecentExchangeRate
{
    public class GetRecenetExchangeRateQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) :
        IRequestHandler<GetRecentExchangeRateQuery, List<ExchangeRateHeaderDeltaDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<List<ExchangeRateHeaderDeltaDto>> Handle(
            GetRecentExchangeRateQuery request,
            CancellationToken cancellationToken)
        {
            var headers = await _unitOfWork.ExchangeRateHeaders.GetRecentWithDetailsAsync(5);
            var result = new List<ExchangeRateHeaderDeltaDto>();

            for (int i = 0; i < headers.Count; i++)
            {
                var current = headers[i];
                var previous = i < headers.Count - 1 ? headers[i + 1] : null;

                var headerDto = _mapper.Map<ExchangeRateHeaderDeltaDto>(current);
                
                foreach (var detail in headerDto.Details)
                {
                    var prevDetail = previous?.Details
                        .FirstOrDefault(d => d.CurrencyCode == detail.CurrencyCode);

                    var delta = prevDetail != null
                        ? detail.Rate - prevDetail.Rate
                        : 0m;

                    var deltaPercentage = (prevDetail != null && prevDetail.Rate != 0)
                        ? (delta / prevDetail.Rate) * 100m
                        : 0m;

                    detail.Delta = Math.Round(delta, 4, MidpointRounding.AwayFromZero);
                    detail.DeltaPercentage = Math.Round(deltaPercentage, 2, MidpointRounding.AwayFromZero);
                }
                result.Add(headerDto);
            }
            return result;
        }
    }
}
