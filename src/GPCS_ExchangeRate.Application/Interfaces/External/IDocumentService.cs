using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;

namespace GPCS_ExchangeRate.Application.Interfaces.External
{
    public interface IDocumentService
    {
        Task<DocumentDto?> GetByIdAsync(int id , CancellationToken ct = default);
        Task<DocumentDto?> CreateAsync(CreateDocumentRequest request, CancellationToken ct = default);
        Task<DocumentDto?> UpdateAsync(int id, UpdateDocumentRequest request, CancellationToken ct = default);
        Task<DocumentDto?> DeleteAsync(int id, CancellationToken ct = default);

        Task<DocumentDto?> SubmitAsync(int id, NotRequireComment request, CancellationToken ct = default);
        Task<DocumentDto?> ApproveAsync(int id, NotRequireComment request, CancellationToken ct = default);
        Task<DocumentDto?> RejectAsync(int id, RequireComment request, CancellationToken ct = default);    
        Task<DocumentDto?> CancelAsync(int id, RequireComment request, CancellationToken ct = default);    
        Task<DocumentDto?> ReturnAsync(int id, ReturnRequest request, CancellationToken ct = default);

        Task RollbackSubmitAsync(int id, RollbackRequest request, CancellationToken ct = default);
        Task RollbackApproveAsync(int id, RollbackRequest request, CancellationToken ct = default);
        Task RollbackRejectAsync(int id, RollbackRequest request, CancellationToken ct = default);
        Task RollbackCancelAsync(int id, RollbackRequest request, CancellationToken ct = default);
        Task RollbackReturnAsync(int id, RollbackRequest request, CancellationToken ct = default);
    }
}
