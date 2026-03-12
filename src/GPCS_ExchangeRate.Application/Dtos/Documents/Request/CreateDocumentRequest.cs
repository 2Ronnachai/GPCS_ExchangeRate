namespace GPCS_ExchangeRate.Application.Dtos.Documents.Request
{
    public class CreateDocumentRequest
    {
        public string? Title { get; set; }
        public string? DocumentNo { get; set; }

        public string? DocumentType { get; set; }
        public int? DocumentTypeId { get; set; }

        public DateTime? EffectiveDate { get; set; } = DateTime.UtcNow;
        public bool IsUrgent { get; set; } = false;
        public string? Remarks { get; set; }

        // For ExternalDocumentCreation
        public string? SourceDocumentId { get; set; }
        public string? SourceUrl { get; set; }

        // For DocumentHistory
        public string? Comment { get; set; }

        // For CreateWorkflowInstanceRequest
        public CreateWorkflowInstanceRequest WorkflowCreateRequest { get; set; } = null!;
    }
}
