namespace GPCS_ExchangeRate.Application.Dtos.Documents.Request
{
    public class CreateWorkflowInstanceRequest
    {
        public int RouteId { get; set; }
        public string NIdRequester { get; set; } = null!;

        // For ResolveWorkflowRouteAsync
        public List<int> OrganizationalUnitIds { get; set; } = [];
        public List<string>? OrganizationalUnitCodes { get; set; } = null;
        public Dictionary<string, object>? ConditionalData { get; set; } = null;
    }
}
