using MediatR;

namespace GPCS_ExchangeRate.Application.Features.ExchangeRates.Commands.Common;

/// <summary>
/// Marker interface for document workflow action commands (Approve, Reject, Cancel, Return).
/// </summary>
public interface IDocumentActionCommand : IRequest
{
    int Id { get; set; }
    string? Comment { get; }
}
