namespace GPCS_ExchangeRate.Application.Dtos.Documents.Request
{
    public class RequireComment
    {
        public string Comment { get; set; } = null!;
    }

    // Cancel, Reject, Return
    public class ReturnRequest : RequireComment
    {
        public int? ReturnToStepSequence { get; set; }
    }
}
