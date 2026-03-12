namespace GPCS_ExchangeRate.Domain.Exceptions;

/// <summary>Thrown when the current user does not have permission to perform the operation.</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException() : base("You do not have permission to perform this action.") { }

    public ForbiddenException(string message) : base(message) { }
}
