using GPCS_ExchangeRate.Application.Interfaces.Configurations;

namespace GPCS_ExchangeRate.Infrastructure.Configurations
{
    public class WorkflowConfiguration : IWorkflowConfiguration
    {
        public int RouteId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
    }
}
