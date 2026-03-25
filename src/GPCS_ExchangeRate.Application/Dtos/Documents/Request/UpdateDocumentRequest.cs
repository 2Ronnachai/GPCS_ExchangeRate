namespace GPCS_ExchangeRate.Application.Dtos.Documents.Request
{
    public class UpdateDocumentRequest
    {
        public int DocumentId { get; set; }
        public string? Title { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public bool IsUrgent { get; set; }
        public string? Remarks { get; set; }
        public string? Comment { get; set; }

        public string? SourceDocumentId { get; set; }
        public string? SourceUrl { get; set; }
        public UpdateWorkflowInstanceRequest? WorkflowUpdateRequest { get; set; }
    }
}
