using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;

namespace GPCS_ExchangeRate.Application.Dtos.Documents
{
    public class DocumentStepDto
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }

        public int SequenceNo { get; set; }
        public string StepName { get; set; } = null!;
        public string ExecutionMode { get; set; } = null!;
        public string CompletionRule { get; set; } = null!;

        public bool AllowReturn { get; set; }
        public string ReturnBehavior { get; set; } = null!;
        public int? ReturnStepSequence { get; set; }

        public bool IsFinalStep { get; set; }
        public string StepStatus { get; set; } = null!;

        public List<DocumentStepAssignmentDto> StepAssignments { get; set; } = [];
    }
}
