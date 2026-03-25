using AutoMapper;
using GPCS_ExchangeRate.Application.Common.Helpers;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetRecentExchangeRate
{
    public class GetRecenetExchangeRateQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper) :
        IRequestHandler<GetRecentExchangeRateQuery, List<ExchangeRateHeaderDeltaDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<List<ExchangeRateHeaderDeltaDto>> Handle(
            GetRecentExchangeRateQuery request,
            CancellationToken cancellationToken)
        {
            var headers = await _unitOfWork.ExchangeRateHeaders.GetRecentCompletedWithDetailsAsync(5);
            return ExchangeRateHeaderDeltaBuilder.Build(headers, _mapper);
        }
    }
}
