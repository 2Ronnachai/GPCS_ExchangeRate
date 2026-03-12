using AutoMapper;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRateById;

public class GetExchangeRateByIdQueryHandler : IRequestHandler<GetExchangeRateByIdQuery, ExchangeRateHeaderDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExchangeRateByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExchangeRateHeaderDto?> Handle(
        GetExchangeRateByIdQuery request,
        CancellationToken cancellationToken)
    {
        var header = await _unitOfWork.ExchangeRateHeaders.GetWithDetailsAsync(request.Id);
        return header is null ? null : _mapper.Map<ExchangeRateHeaderDto>(header);
    }
}
