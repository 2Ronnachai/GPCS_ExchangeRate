using AutoMapper;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetAllExchangeRate
{
    public class GetExchangeRateQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) :
        IRequestHandler<GetExchangeRateQuery, List<ExchangeRateHeaderDetailDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<List<ExchangeRateHeaderDetailDto>> Handle(
            GetExchangeRateQuery request,
            CancellationToken cancellationToken)
        {
            var headers = await _unitOfWork.ExchangeRateHeaders.GetAllWithDetailsAsync();
            return _mapper.Map<List<ExchangeRateHeaderDetailDto>>(headers);
        }
    }
}
