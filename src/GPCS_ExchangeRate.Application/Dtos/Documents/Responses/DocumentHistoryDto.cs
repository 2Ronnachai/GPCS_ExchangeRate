namespace GPCS_ExchangeRate.Application.Dtos.Documents.Responses
{
    public class DocumentHistoryDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = null!;
        public int? FromStepSequence { get; set; }
        public int? ToStepSequence { get; set; }
        public string? StepName { get; set; }
        public string ActionBy { get; set; } = null!;
        public string? ActionByName { get; set; }
        public string? Remarks { get; set; }
        public DateTime ActionAt { get; set; }
    }
}
