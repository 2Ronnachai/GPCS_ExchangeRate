using AutoMapper;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.CreateExchangeRate;

public class CreateExchangeRateCommandHandler : IRequestHandler<CreateExchangeRateCommand, ExchangeRateHeaderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateExchangeRateCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ExchangeRateHeaderDto> Handle(
        CreateExchangeRateCommand request,
        CancellationToken cancellationToken)
    {
        // Parse "yyyyMM" string → DateTime (first day of the month)
        if (!TryParsePeriod(request.Period, out var periodDate))
            throw new ArgumentException($"Invalid period format '{request.Period}'. Expected format: yyyyMM (e.g. 202603)");

        var header = new ExchangeRateHeader
        {
            Period = periodDate,
            Details = request.Items.Select(item => new ExchangeRateDetail
            {
                CurrencyCode = item.CurrencyCode.ToUpperInvariant(),
                Rate = item.Rate,
                Rate2Digit = Math.Round(item.Rate, 2, MidpointRounding.AwayFromZero),
                Rate4Digit = Math.Round(item.Rate, 4, MidpointRounding.AwayFromZero),
            }).ToList()
        };

        await _unitOfWork.ExchangeRateHeaders.AddAsync(header);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload header with details for mapping
        var result = await _unitOfWork.ExchangeRateHeaders.GetWithDetailsAsync(header.Id);
        return _mapper.Map<ExchangeRateHeaderDto>(result);
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
