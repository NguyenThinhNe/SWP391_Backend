using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IClaimService
    {
        /// Create a new warranty claim 
        Task<ClaimResponse> CreateClaimAsync(ClaimRequest request, Guid technicianId);

        /// Get claim by ID with full details
        Task<ClaimResponse> GetClaimByIdAsync(Guid claimId);

        /// Get list of claims with filters
        Task<ICollection<ClaimResponse>> GetClaimsAsync(
            WarrantyClaimStatus? status = null,
            Guid? serviceCenterId = null);

        /// Start reviewing claim (EVM Staff) - Change status to InProgress
        Task<ClaimResponse> StartReviewAsync(Guid claimId, Guid evmStaffId);

        /// Approve claim (EVM Staff) - Change status to Completed
        Task<ClaimResponse> ApproveClaimAsync(Guid claimId, Guid evmStaffId);

        /// Reject claim (EVM Staff)
        Task<ClaimResponse> RejectClaimAsync(Guid claimId, Guid evmStaffId, string rejectionReason);

        /// Update claim status
        Task<ClaimResponse> UpdateClaimStatusAsync(Guid claimId, WarrantyClaimStatus newStatus, Guid userId);

        /// Check if vehicle is eligible for warranty
        Task<bool> ValidateWarrantyEligibilityAsync(string vin, Guid policyId);

        /// Get claims by technician
        Task<ICollection<ClaimResponse>> GetClaimsByTechnicianAsync(Guid technicianId);

        /// Get pending claims for review
        Task<ICollection<ClaimResponse>> GetPendingClaimsAsync();

        /// Get overdue claims
        Task<ICollection<ClaimResponse>> GetOverdueClaimsAsync();
    }
}
