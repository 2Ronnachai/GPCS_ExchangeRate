namespace GPCS_ExchangeRate.Application.Dtos.Documents.Responses
{
    /// <summary>Envelope returned by the External Document Control API.</summary>
    public sealed class ExternalApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? ErrorCode { get; init; }
    }
}
