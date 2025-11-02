using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IClaimService
    {
        /// Create a new warranty claim 
        Task<ClaimResponse> CreateClaimAsync(ClaimRequest request, Guid technicianId);
        /// Update existing claim
        Task<ClaimResponse> UpdateClaimAsync(Guid claimId, UpdateClaimPartItemsRequest request);
        /// Get claim by ID with full details
        Task<ClaimResponse> GetClaimByIdAsync(Guid claimId);

        /// Get list of claims by service center id
        Task<ICollection<ClaimResponse>> GetClaimsAsync(
            Guid? serviceCenterId = null);
        /// Get claims by status
        Task<ICollection<ClaimResponse>> GetClaimsByStatusAsync(WarrantyClaimStatus status);

        /// Approve claim (EVM Staff) - Change status to Completed
        Task<ClaimResponse> ApproveClaimAsync(Guid claimId, Guid evmStaffId);

        /// Reject claim (EVM Staff)
        Task<ClaimResponse> RejectClaimAsync(Guid claimId, Guid evmStaffId, string rejectionReason);

        /// Update claim status
        Task<ClaimResponse> UpdateClaimStatusAsync(Guid claimId, WarrantyClaimStatus newStatus, Guid userId);

        /// Check if vehicle is eligible for warranty- based on VIN and policy ID
        Task<bool> ValidateWarrantyEligibilityAsync(string vin, Guid policyId);

        /// Get claims by technician
        Task<ICollection<ClaimResponse>> GetClaimsByUserAsync(Guid userId);

        //Delete claim by ID
        Task<bool> DeleteClaimAsync(Guid claimId);
    }
}
