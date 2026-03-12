namespace GPCS_ExchangeRate.Domain.Common.Interfaces
{
    public interface IAuditableEntity
    {
        string? CreatedBy { get; set; }
        DateTime CreatedAt { get; set; }
        string? UpdatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
