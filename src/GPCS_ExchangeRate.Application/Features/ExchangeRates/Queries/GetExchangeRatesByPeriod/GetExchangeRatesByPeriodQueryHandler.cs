using AutoMapper;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Queries.GetExchangeRatesByPeriod;

public class GetExchangeRatesByPeriodQueryHandler
    : IRequestHandler<GetExchangeRatesByPeriodQuery, ExchangeRateHeaderDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExchangeRatesByPeriodQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExchangeRateHeaderDto?> Handle(
        GetExchangeRatesByPeriodQuery request,
        CancellationToken cancellationToken)
    {
        if (!TryParsePeriod(request.Period, out var periodDate))
            throw new ArgumentException($"Invalid period format '{request.Period}'. Expected format: yyyyMM (e.g. 202603)");

        var header = await _unitOfWork.ExchangeRateHeaders.GetByPeriodAsync(periodDate);
        return header is null ? null : _mapper.Map<ExchangeRateHeaderDto>(header);
    }

    private static bool TryParsePeriod(string period, out DateTime result)
    {
        result = default;
        if (period.Length != 6) return false;
        if (!int.TryParse(period[..4], out var year)) return false;
        if (!int.TryParse(period[4..], out var month)) return false;
        if (month < 1 || month > 12) return false;

        result = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        return true;
    }
}
