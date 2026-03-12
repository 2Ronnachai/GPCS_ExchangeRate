namespace GPCS_ExchangeRate.Infrastructure.Configurations
{
    public class DocumentApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int Timeout { get; set; } = 30; // Default to 30 seconds
        public string? ApiKey { get; set; }
    }
}
