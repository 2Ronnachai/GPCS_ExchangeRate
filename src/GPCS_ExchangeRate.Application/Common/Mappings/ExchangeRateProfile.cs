using AutoMapper;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

namespace GPCS_ExchangeRate.Application.Common.Mappings;

public class ExchangeRateProfile : Profile
{
    public ExchangeRateProfile()
    {
        CreateMap<ExchangeRateHeader, ExchangeRateHeaderDetailDto>()
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToString("yyyyMM")));

        CreateMap<ExchangeRateDetail, ExchangeRateDetailDto>();

        CreateMap<ExchangeRateHeader, ExchangeRateHeaderDeltaDto>()
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToString("yyyyMM")));

        CreateMap<ExchangeRateDetail, ExchangeRateDetailDeltaDto>()
            .ForMember(dest => dest.Delta, opt => opt.Ignore())
            .ForMember(dest => dest.DeltaPercentage, opt => opt.Ignore());
    }
}
