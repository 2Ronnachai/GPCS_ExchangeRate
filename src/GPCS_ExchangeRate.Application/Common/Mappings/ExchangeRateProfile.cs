using AutoMapper;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

namespace GPCS_ExchangeRate.Application.Common.Mappings;

public class ExchangeRateProfile : Profile
{
    public ExchangeRateProfile()
    {
        CreateMap<ExchangeRateHeader, ExchangeRateHeaderDto>()
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToString("yyyyMM")));

        CreateMap<ExchangeRateDetail, ExchangeRateDetailDto>();
    }
}
