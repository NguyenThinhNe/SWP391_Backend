using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.BLL.Services.Implements
{
    public class ClaimService: IClaimService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IMapper _mapper;

        public ClaimService(
            IUnitOfWork<WarrantyDbContext> unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Get Claims

        public async Task<ClaimResponse> GetClaimByIdAsync(Guid claimId)
        {
            var claim = await _unitOfWork.Context.WarrantyClaims
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Customer)
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Campaign)
                .Include(c => c.PartItems)
                    .ThenInclude(pi => pi.Part)
                .Include(c => c.WarrantyPolicy)
                .Include(c => c.User)
                    .ThenInclude(u => u.ServiceCenter)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClaimId == claimId);

            if (claim == null)
            {
                throw new KeyNotFoundException($"Claim with ID {claimId} not found");
            }

            return _mapper.Map<ClaimResponse>(claim);
        }

        public async Task<ICollection<ClaimResponse>> GetClaimsAsync(
            
            Guid? serviceCenterId = null)
        {
            
            var query = _unitOfWork.Context.WarrantyClaims
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Customer)
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Campaign)
                .Include(c => c.PartItems)
                    .ThenInclude(pi => pi.Part)
                .Include(c => c.WarrantyPolicy)
                .Include(c => c.User)
                    .ThenInclude(u => u.ServiceCenter)
                .AsNoTracking()
                .AsQueryable();

            

            if (serviceCenterId.HasValue)
            {
                query = query.Where(c => c.User.ServiceCenterId == serviceCenterId.Value);
            }

            // Execute and order
            var claims = await query
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            return _mapper.Map<ICollection<ClaimResponse>>(claims);
        }

        public async Task<ICollection<ClaimResponse>> GetClaimsByServiceCenterAsync(Guid serviceCenterId)
        {
            // Validate service center exists
            var serviceCenterExists = await _unitOfWork.Context.ServiceCenters
                .AnyAsync(sc => sc.CenterId == serviceCenterId);

            if (!serviceCenterExists)
            {
                throw new KeyNotFoundException($"Service center with ID {serviceCenterId} not found");
            }

            var claims = await _unitOfWork.Context.WarrantyClaims
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Customer)
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Campaign)
                .Include(c => c.PartItems)
                    .ThenInclude(pi => pi.Part)
                .Include(c => c.WarrantyPolicy)
                .Include(c => c.User)
                    .ThenInclude(u => u.ServiceCenter)
                .AsNoTracking()
                .Where(c => c.User.ServiceCenterId == serviceCenterId)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            return _mapper.Map<ICollection<ClaimResponse>>(claims);
        }

        public async Task<ICollection<ClaimResponse>> GetClaimsByTechnicianAsync(Guid technicianId)
        {
            var claims = await _unitOfWork.Context.WarrantyClaims
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Customer)
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Campaign)
                .Include(c => c.PartItems)
                    .ThenInclude(pi => pi.Part)
                .Include(c => c.WarrantyPolicy)
                .Include(c => c.User)
                    .ThenInclude(u => u.ServiceCenter)
                .AsNoTracking()
                .Where(c => c.UserId == technicianId)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            return _mapper.Map<ICollection<ClaimResponse>>(claims);
        }

        public async Task<ICollection<ClaimResponse>> GetClaimsByStatusAsync(WarrantyClaimStatus status)
        {
            var claims = await _unitOfWork.Context.WarrantyClaims
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Customer)
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Campaign)
                .Include(c => c.PartItems)
                    .ThenInclude(pi => pi.Part)
                .Include(c => c.WarrantyPolicy)
                .Include(c => c.User)
                    .ThenInclude(u => u.ServiceCenter)
                .AsNoTracking()
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            return _mapper.Map<ICollection<ClaimResponse>>(claims);
        }

        #endregion

        #region Create Claim



        #endregion

        #region Update Claim Status

        public async Task<ClaimResponse> StartReviewAsync(Guid claimId, Guid evmStaffId)
        {
            return await UpdateClaimStatusAsync(claimId, WarrantyClaimStatus.InProgress, evmStaffId);
        }

        public async Task<ClaimResponse> ApproveClaimAsync(Guid claimId, Guid evmStaffId)
        {
            return await UpdateClaimStatusAsync(claimId, WarrantyClaimStatus.Completed, evmStaffId);
        }

        public async Task<ClaimResponse> RejectClaimAsync(Guid claimId, Guid evmStaffId, string rejectionReason)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var claim = await _unitOfWork.Context.WarrantyClaims
                    .FirstOrDefaultAsync(c => c.ClaimId == claimId);

                if (claim == null)
                {
                    throw new KeyNotFoundException($"Claim with ID {claimId} not found");
                }

                ValidateStatusTransition(claim.Status, WarrantyClaimStatus.Completed);

                claim.Status = WarrantyClaimStatus.Completed;
                

                _unitOfWork.Context.WarrantyClaims.Update(claim);
                await _unitOfWork.SaveChangesAsync();

                return await GetClaimByIdAsync(claimId);
            });
        }

        public async Task<ClaimResponse> UpdateClaimStatusAsync(
            Guid claimId,
            WarrantyClaimStatus newStatus,
            Guid userId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var claim = await _unitOfWork.Context.WarrantyClaims
                    .FirstOrDefaultAsync(c => c.ClaimId == claimId);

                if (claim == null)
                {
                    throw new KeyNotFoundException($"Claim with ID {claimId} not found");
                }

                ValidateStatusTransition(claim.Status, newStatus);

                claim.Status = newStatus;
                

                _unitOfWork.Context.WarrantyClaims.Update(claim);
                await _unitOfWork.SaveChangesAsync();

                return await GetClaimByIdAsync(claimId);
            });
        }

        #endregion

        #region Validation

        public async Task<bool> ValidateWarrantyEligibilityAsync(string vin, Guid policyId)
        {
            var vehicle = await _unitOfWork.Context.CustomerVehicles
                .FirstOrDefaultAsync(v => v.VIN == vin);

            if (vehicle == null)
            {
                return false;
            }

            var policy = await _unitOfWork.Context.Policies
                .FirstOrDefaultAsync(p => p.PolicyId == policyId);

            if (policy == null)
            {
                return false;
            }

            var warrantyEndDate = vehicle.PurchaseDate.AddMonths(policy.DurationMonth);
            if (DateTime.Now > warrantyEndDate)
            {
                return false;
            }

            return true;
        }

        private void ValidateStatusTransition(
            WarrantyClaimStatus currentStatus,
            WarrantyClaimStatus newStatus)
        {
            var validTransitions = new Dictionary<WarrantyClaimStatus, List<WarrantyClaimStatus>>
            {
                {
                    WarrantyClaimStatus.Pending,
                    new List<WarrantyClaimStatus>
                    {
                        WarrantyClaimStatus.InProgress,
                        WarrantyClaimStatus.Overdue
                    }
                },
                {
                    WarrantyClaimStatus.InProgress,
                    new List<WarrantyClaimStatus>
                    {
                        WarrantyClaimStatus.Completed,
                        WarrantyClaimStatus.Overdue
                    }
                },
                {
                    WarrantyClaimStatus.Completed,
                    new List<WarrantyClaimStatus>()
                },
                {
                    WarrantyClaimStatus.Overdue,
                    new List<WarrantyClaimStatus>
                    {
                        WarrantyClaimStatus.InProgress,
                        WarrantyClaimStatus.Completed
                    }
                }
            };

            if (!validTransitions.ContainsKey(currentStatus) ||
                !validTransitions[currentStatus].Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"Invalid status transition from {currentStatus} to {newStatus}");
            }
        }

        #endregion
    }
}
