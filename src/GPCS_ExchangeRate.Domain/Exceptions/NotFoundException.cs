namespace GPCS_ExchangeRate.Domain.Exceptions;

/// <summary>Thrown when a requested resource cannot be found.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} with key '{key}' was not found.") { }
}
