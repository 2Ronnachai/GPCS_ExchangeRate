using AutoMapper;
using GPCS_ExchangeRate.Application.Features.Dashboards.Dto;
using GPCS_ExchangeRate.Domain.Entities;

namespace GPCS_ExchangeRate.Application.Common.Mappings
{
    public class DashboardProfile : Profile
    {
        public DashboardProfile()
        {
            CreateMap<ExchangeRateHeader, PendingApprovalDocumentDto>()
                .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.ToString("yyyyMM")))
                .ForMember(dest => dest.CanApprove, opt => opt.Ignore())
                .ForMember(dest => dest.CanReject, opt => opt.Ignore())
                .ForMember(dest => dest.CanReturn, opt => opt.Ignore())
                .ForMember(dest => dest.CanAdd, opt => opt.Ignore())
                .ForMember(dest => dest.CanEdit, opt => opt.Ignore())
                .ForMember(dest => dest.CanRemove, opt => opt.Ignore());
        }
    }
}
