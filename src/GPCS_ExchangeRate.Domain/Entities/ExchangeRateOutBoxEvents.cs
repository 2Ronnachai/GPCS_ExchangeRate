using GPCS_ExchangeRate.Domain.Common;

namespace GPCS_ExchangeRate.Domain.Entities
{
    public class ExchangeRateOutBoxEvents : AuditableEntity
    {
        public string EventType { get; set; } = null!;
        public string Payload { get; set; } = null!; // Json serialized event data
        public OutBoxStatus Status { get; set; } = OutBoxStatus.Pending;
        public int RetryCount { get; set; } = 0;
        public int MaxRetryAttempts { get; set; } = 3;
        public string? ErrorMessage { get; set; }
    }

    public enum OutBoxStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }
}
