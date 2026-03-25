using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;

namespace GPCS_ExchangeRate.Application.Features.Dashboards.Dto
{
    public class DashboardDto
    {
        public List<string> UserRoles { get; set; } = [];

        public SummaryDashboardDto Summary { get; set; } = new SummaryDashboardDto();
        public PendingApprovalDashboardDto PendingApproval { get; set; } = new PendingApprovalDashboardDto();
        
        public List<ExchangeRateHeaderDeltaDto> RecentCompletedDocuments { get; set; } = [];
    }

    public class SummaryDashboardDto
    {
        public int TotalDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int ApprovedDocuments { get; set; }
        public int DraftDocuments { get; set; }
    }

    public class PendingApprovalDashboardDto
    {
        public List<PendingApprovalDocumentDto> PendingApprovalDocuments { get; set; } = [];
    }

    public class PendingApprovalDocumentDto : ExchangeRateHeaderDeltaDto
    {
        public string Title { get; set; } = string.Empty;
        public string PeriodLabel { get; set; } = string.Empty;
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanReturn { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanRemove { get; set; }
    }
}
