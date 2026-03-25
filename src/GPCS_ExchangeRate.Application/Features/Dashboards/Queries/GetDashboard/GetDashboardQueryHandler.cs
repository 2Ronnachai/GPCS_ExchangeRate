using AutoMapper;
using GPCS_ExchangeRate.Application.Common.Helpers;
using GPCS_ExchangeRate.Application.Dtos.Documents;
using GPCS_ExchangeRate.Application.Dtos.Documents.Responses;
using GPCS_ExchangeRate.Application.Features.Dashboards.Dto;
using GPCS_ExchangeRate.Application.Features.ExchangeRates.Dto;
using GPCS_ExchangeRate.Application.Interfaces.External;
using GPCS_ExchangeRate.Domain.Entities;
using GPCS_ExchangeRate.Domain.Exceptions;
using GPCS_ExchangeRate.Domain.Interfaces;
using MediatR;

namespace GPCS_ExchangeRate.Application.Features.Dashboards.Queries.GetDashboard
{
    public class GetDashboardQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserAccountService userAccountService,
        IDocumentService documentService) :
        IRequestHandler<GetDashboardQuery, DashboardDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IUserAccountService _userAccountService = userAccountService;
        private readonly IDocumentService _documentService = documentService;

        public async Task<DashboardDto> Handle(
            GetDashboardQuery request,
            CancellationToken cancellationToken)
        {
            // Get user roles
            var user = await _userAccountService.GetByNIdAsync(request.UserName, cancellationToken)
                ?? throw new NotFoundException($"User with NId {request.UserName} not found.");

            // Batch queries
            var pendingApprovals = await _unitOfWork.ExchangeRateHeaders
                .GetPendingApprovalWithDetailsAsync();

            var recentCompleted = await _unitOfWork.ExchangeRateHeaders
                .GetRecentCompletedWithDetailsAsync(5);

            // Fetch documents
            var documentIds = pendingApprovals
                .Where(a => a.DocumentId.HasValue)
                .Select(a => a.DocumentId!.Value)
                .Distinct()
                .ToList();

            var documents = await _documentService
                .GetByIdsAsync(documentIds, cancellationToken); // batch call
            var documentMap = documents?.ToDictionary(d => d.Id) ?? [];

            // Fetch users
            var creatorNIds = (documents ?? [])
                .Where(d => d.CreatedBy != null)
                .Select(d => d.CreatedBy!)
                .Distinct()
                .ToList();

            var users = await _userAccountService
                .GetByNIdsAsync(creatorNIds, cancellationToken); // batch call
            var userMap = users.ToDictionary(u => u.NId);

            // Fetch previous completed headers for all pending approvals
            var periods = pendingApprovals.Select(a => a.Period).Distinct().ToList();
            var previousHeaders = await _unitOfWork.ExchangeRateHeaders
                .GetPreviousCompletedBatchAsync(periods, cancellationToken); // batch
            var previousMap = previousHeaders.ToDictionary(h => h.Period);

            // Summary data
            var allHeaders = await _unitOfWork.ExchangeRateHeaders.GetAllAsync();

            var dashboardDto = new DashboardDto
            {
                UserRoles = user.Roles,
                Summary =
                {
                    TotalDocuments    = allHeaders.Count,
                    PendingDocuments  = allHeaders.Count(h => h.CompletedAt == null
                                            && h.DocumentStatus != "Draft"),
                    ApprovedDocuments = allHeaders.Count(h => h.CompletedAt.HasValue),
                    DraftDocuments    = allHeaders.Count(h => h.DocumentStatus == "Draft"),
                },
                RecentCompletedDocuments = ExchangeRateHeaderDeltaBuilder.Build(recentCompleted, _mapper)
            };

            foreach (var approval in pendingApprovals)
            {
                var pendingDto = _mapper.Map<PendingApprovalDocumentDto>(approval);

                // Delta calculation
                if (previousMap.TryGetValue(approval.Period.AddMonths(-1), out var prevHeader))
                {
                    foreach (var detail in pendingDto.Details)
                    {
                        var prevDetail = prevHeader.Details
                            .FirstOrDefault(d => d.CurrencyCode == detail.CurrencyCode);
                        (detail.Delta, detail.DeltaPercentage) =
                            ExchangeRateDeltaCalculator.Calculate(detail.Rate, prevDetail?.Rate);
                    }
                }

                if (!approval.DocumentId.HasValue
                    || !documentMap.TryGetValue(approval.DocumentId.Value, out var document))
                {
                    dashboardDto.PendingApproval.PendingApprovalDocuments.Add(pendingDto);
                    continue;
                }

                pendingDto.Title = document.Title;
                pendingDto.PeriodLabel = approval.Period.ToString("MMMM yyyy");
                pendingDto.CreatedBy = userMap.TryGetValue(document.CreatedBy ?? "", out var creator)
                                            ? creator.FullName
                                            : string.Empty;

                var currentStep = document.Steps
                    .FirstOrDefault(s => s.SequenceNo == document.CurrentStepSequence);

                var (canApprove, canReject, canReturn, canAdd, canEdit, canRemove) =
                    DeterminePermissions(document, currentStep, request.UserName, user.Roles);

                pendingDto.CanApprove = canApprove;
                pendingDto.CanReject = canReject;
                pendingDto.CanReturn = canReturn;
                pendingDto.CanAdd = canAdd;
                pendingDto.CanEdit = canEdit;
                pendingDto.CanRemove = canRemove;

                dashboardDto.PendingApproval.PendingApprovalDocuments.Add(pendingDto);
            }

            return dashboardDto;
        }

        private static (bool canApprove, bool canReject, bool canReturn,
                bool canAdd, bool canEdit, bool canRemove)
                DeterminePermissions(
                    DocumentDto document,
                    DocumentStepDto? currentStep,
                    string userNId,
                    List<string> userRoles)
        {
            var none = (false, false, false, false, false, false);

            if (document == null || currentStep == null)
                return none;

            if (document.DocumentStatus is "Completed" or "Rejected" or "Cancelled")
                return none;

            var isCreator = document.CreatedBy == userNId;

            if (document.DocumentStatus is "Draft" || document.CurrentStepSequence == 1)
            {
                var canEdit = isCreator
                             || userRoles.Contains("Administrator");
                return (false, false, false, canEdit, canEdit, canEdit);
            }

            if (userRoles.Contains("Administrator"))
                return (true, true, true, true, true, true);

            if (currentStep.StepStatus != "InProgress")
                return none;

            if (document.DocumentStatus == "Draft")
            {
                return (false, false, false, isCreator, isCreator, isCreator);
            }

            var assignment = currentStep.StepAssignments
                .FirstOrDefault(a => a.NId == userNId && a.Status == "Pending");

            if (assignment == null)
                return none;

            var isActionRole = assignment.AssignmentType is "Approver" or "Verifier";
            var isDataRole = assignment.AssignmentType == "User";

            return(
                canApprove: isActionRole,
                canReject: isActionRole,
                canReturn: isActionRole && currentStep.AllowReturn,
                canAdd: isDataRole,
                canEdit: isDataRole,
                canRemove: isDataRole
            );
        }
    }
}
