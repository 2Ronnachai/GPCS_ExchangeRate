using GPCS_ExchangeRate.Application.Features.Dashboards.Dto;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.Dashboards.Queries.GetDashboard
{
    public class GetDashboardQuery : IRequest<DashboardDto>
    {
        public string UserName { get; set; } = null!;
    }
}
