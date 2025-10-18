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
        Task<ClaimResponse> CreateClaimAsync(ClaimRequest request, Guid userId);
        Task<IEnumerable<ClaimResponse>> GetAllClaimsAsync();
        Task<ClaimResponse> GetClaimByIdAsync(Guid claimId);
        //Task<bool> UpdateClaimStatusAsync(Guid claimId, WarrantyClaimStatus newStatus);
        //Task<bool> DeleteClaimAsync(Guid claimId);
    }
}
