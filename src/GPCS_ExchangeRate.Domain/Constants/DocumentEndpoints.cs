namespace GPCS_ExchangeRate.Domain.Constants
{
    public static class DocumentEndpoints
    {
        // Query Endpoints
        public const string GetAllDocuments = ""; // GET
        public const string GetDocumentById = "{id:int}"; // GET

        // Command Endpoints
        public const string CreateDocument =""; // POST
        public const string UpdateDocument = "{id:int}"; // PUT
        public const string DeleteDocument = "{id:int}"; // DELETE

        // Action Handler Endpoints
        public const string SubmitDocument = "{id:int}/submit"; // POST
        public const string ApproveDocument = "{id:int}/approve"; // POST
        public const string RejectDocument = "{id:int}/reject"; // POST
        public const string CancelDocument = "{id:int}/cancel"; // POST
        public const string ReturnDocument = "{id:int}/return"; // POST

        // Rollback Endpoints
        public const string RollbackSubmit = "{id:int}/rollback-submit"; // POST
        public const string RollbackApprove = "{id:int}/rollback-approve"; // POST
        public const string RollbackReject = "{id:int}/rollback-reject"; // POST
        public const string RollbackCancel = "{id:int}/rollback-cancel"; // POST
        public const string RollbackReturn = "{id:int}/rollback-return"; // POST
    }
}
