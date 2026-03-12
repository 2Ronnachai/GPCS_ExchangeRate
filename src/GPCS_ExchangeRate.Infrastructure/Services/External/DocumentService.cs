using GPCS_ExchangeRate.Application.Dtos.Documents.Request;
using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Constants;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace GPCS_ExchangeRate.Infrastructure.Services.External
{
    public class DocumentService(
        HttpClient httpClient,
        ILogger<DocumentService> logger) : IDocumentService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<DocumentService> _logger = logger;

        // ── CRUD ──────────────────────────────────────────────────────────────────

        public Task<DocumentDto?> GetByIdAsync(int id, CancellationToken ct = default)
            => SendAsync<DocumentDto>(HttpMethod.Get, BuildUrl(DocumentEndpoints.GetDocumentById, id), null, ct);

        public Task<DocumentDto?> CreateAsync(CreateDocumentRequest request, CancellationToken ct = default)
            => SendAsync<DocumentDto>(HttpMethod.Post, BuildUrl(DocumentEndpoints.CreateDocument), request, ct);

        public Task<DocumentDto?> UpdateAsync(int id, UpdateDocumentRequest request, CancellationToken ct = default)
            => SendAsync<DocumentDto>(HttpMethod.Put, BuildUrl(DocumentEndpoints.UpdateDocument, id), request, ct);

        public Task<DocumentDto?> DeleteAsync(int id, CancellationToken ct = default)
            => SendAsync<DocumentDto>(HttpMethod.Delete, BuildUrl(DocumentEndpoints.DeleteDocument, id), null, ct);

        // ── Actions ───────────────────────────────────────────────────────────────

        public Task SubmitAsync(int id, NotRequireComment request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.SubmitDocument, id), request, ct);

        public Task ApproveAsync(int id, NotRequireComment request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.ApproveDocument, id), request, ct);

        public Task RejectAsync(int id, RequireComment request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.RejectDocument, id), request, ct);

        public Task CancelAsync(int id, RequireComment request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.CancelDocument, id), request, ct);

        public Task ReturnAsync(int id, ReturnRequest request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.ReturnDocument, id), request, ct);

        // ── Rollbacks ─────────────────────────────────────────────────────────────

        public Task RollbackSubmitAsync(int id, RollbackRequest request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.RollbackSubmit, id), request, ct);

        public Task RollbackApproveAsync(int id, RollbackRequest request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.RollbackApprove, id), request, ct);

        public Task RollbackRejectAsync(int id, RollbackRequest request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.RollbackReject, id), request, ct);

        public Task RollbackCancelAsync(int id, RollbackRequest request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.RollbackCancel, id), request, ct);

        public Task RollbackReturnAsync(int id, RollbackRequest request, CancellationToken ct = default)
            => SendActionAsync(BuildUrl(DocumentEndpoints.RollbackReturn, id), request, ct);

        // ── Private Helpers ───────────────────────────────────────────────────────

        /// <summary>Sends an HTTP request and deserializes the response into <typeparamref name="T"/>.</summary>
        private async Task<T?> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(method, url);
            if (body is not null)
                request.Content = JsonContent.Create(body);

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External API {Method} {Url} returned {StatusCode}", method, url, response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var envelope = await response.Content
                .ReadFromJsonAsync<ExternalApiResponse<T>>(cancellationToken: ct);

            if (envelope is null || !envelope.Success)
            {
                var msg = envelope?.Message ?? "Unknown error from External Document API.";
                _logger.LogWarning("External API responded with failure: {Message}", msg);
                throw new HttpRequestException(msg);
            }

            return envelope.Data;
        }

        /// <summary>Sends a POST action request that carries no data payload in the response.</summary>
        private async Task SendActionAsync(string url, object body, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body)
            };

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("External API POST {Url} returned {StatusCode}", url, response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            var envelope = await response.Content
                .ReadFromJsonAsync<ExternalApiResponse<object>>( cancellationToken: ct);

            if (envelope is not null && !envelope.Success)
            {
                var msg = envelope.Message ?? "Unknown error from External Document API.";
                _logger.LogWarning("External API action failed: {Message}", msg);
                throw new HttpRequestException(msg);
            }
        }

        // ── URL Builder ───────────────────────────────────────────────────────────

        /// <summary>Replaces the <c>{id:int}</c> route token with the actual id value.</summary>
        private static string BuildUrl(string template, int? id = null)
        {
            if (id is null) return template;
            return template.Replace("{id:int}", id.ToString());
        }
    }
}
