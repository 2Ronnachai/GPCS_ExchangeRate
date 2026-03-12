namespace GPCS_ExchangeRate.Application.Dtos.Documents.Responses
{
    public class DocumentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string DocumentNo { get; set; } = null!;
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string DocumentStatus { get; set; } = null!;
        public DateTime? EffectiveDate { get; set; }
        public bool IsUrgent { get; set; }

        // Optional source information for traceability
        public string? SourceDocumentId { get; set; }
        public string? SourceUrl { get; set; }

        public string Remarks { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Detail 
        public int RouteId { get; set; }
        public int? CurrentStepSequence { get; set; }

        public List<DocumentHistoryDto> RecentHistory { get; set; } = [];
        public List<DocumentStepDto> Steps { get; set; } = [];
    }
}
