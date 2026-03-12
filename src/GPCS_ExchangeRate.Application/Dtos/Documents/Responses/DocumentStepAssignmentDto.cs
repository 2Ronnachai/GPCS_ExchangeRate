namespace GPCS_ExchangeRate.Application.Dtos.Documents.Responses
{
    public class DocumentStepAssignmentDto
    {
        public int Id { get; set; }
        public int WorkflowStepInstanceId { get; set; }
        public string NId { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string AssignmentType { get; set; } = null!;
        public string? PositionName { get; set; }
        public string? OrganizationalUnitName { get; set; }

        public string? Delegator { get; set; }
        public bool IsDelegated { get; set; }
        public string? DelegationReason { get; set; }

        public string Status { get; set; } = null!;
        public DateTime? ActionAt { get; set; }
        public string? Comment { get; set; }
    }
}
