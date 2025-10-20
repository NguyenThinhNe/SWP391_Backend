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
            WarrantyClaimStatus? status = null,
            Guid? serviceCenterId = null)
        {
            // ✅ Sử dụng DbContext trực tiếp cho query phức tạp
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

            // Apply filters
            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

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

        public async Task<ICollection<ClaimResponse>> GetPendingClaimsAsync()
        {
            return await GetClaimsAsync(status: WarrantyClaimStatus.Pending);
        }

        public async Task<ICollection<ClaimResponse>> GetOverdueClaimsAsync()
        {
            return await GetClaimsAsync(status: WarrantyClaimStatus.Overdue);
        }

        #endregion

        #region Create Claim

        public async Task<ClaimResponse> CreateClaimAsync(ClaimRequest request, Guid technicianId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // ✅ Validate technician exists and has ServiceCenter
                var technician = await _unitOfWork.Context.Users
                    .Include(u => u.ServiceCenter)
                    .FirstOrDefaultAsync(u => u.UserId == technicianId);

                if (technician == null)
                {
                    throw new KeyNotFoundException($"Technician with ID {technicianId} not found");
                }

                if (technician.ServiceCenterId == null || technician.ServiceCenter == null)
                {
                    throw new InvalidOperationException($"Technician is not assigned to any service center");
                }

                // Validate vehicle exists
                var vehicle = await _unitOfWork.Context.CustomerVehicles
                    .Include(v => v.Customer)
                    .Include(v => v.Campaign)
                    .FirstOrDefaultAsync(v => v.VIN == request.VIN);

                if (vehicle == null)
                {
                    throw new KeyNotFoundException($"Vehicle with VIN {request.VIN} not found");
                }

                // Update vehicle information
                vehicle.VehicleName = request.VehicleName;
                vehicle.PurchaseDate = request.PurchaseDate;
                vehicle.MileAge = request.Mileage;
                _unitOfWork.Context.CustomerVehicles.Update(vehicle);

                // Find or create part
                var part = await _unitOfWork.Context.Parts
                    .FirstOrDefaultAsync(p => p.PartName == request.PartName);

                if (part == null)
                {
                    part = new Part
                    {
                        PartId = Guid.NewGuid(),
                        PartName = request.PartName,
                        
                    };
                    await _unitOfWork.Context.Parts.AddAsync(part);
                }

                // Get policy
                var policy = await _unitOfWork.Context.Policies
                    .FirstOrDefaultAsync(p => p.PolicyId == request.PolicyId);

                if (policy == null)
                {
                    throw new KeyNotFoundException($"Warranty policy with ID {request.PolicyId} not found");
                }

                // Validate warranty eligibility
                var isEligible = await ValidateWarrantyEligibilityAsync(request.VIN, policy.PolicyId);
                if (!isEligible)
                {
                    throw new InvalidOperationException("Vehicle is not eligible for warranty claim. Warranty may have expired.");
                }

                // Create warranty claim
                var claim = new WarrantyClaim
                {
                    ClaimId = Guid.NewGuid(),
                    ClaimDate = request.ClaimDate == default ? DateTime.UtcNow : request.ClaimDate,
                    Status = WarrantyClaimStatus.Pending,
                    IssueDescription = request.IssueDescription,
                    ClaimDescription = request.ClaimDescription,
                    VIN = request.VIN,
                    UserId = technicianId,
                    PolicyId = policy.PolicyId

                };
                claim.CustomerVehicle = vehicle;
                claim.WarrantyPolicy = policy;
                claim.User = technician;
                await _unitOfWork.Context.WarrantyClaims.AddAsync(claim);

                // Create PartItem
                var partItem = new PartItem
                {
                    PartItemId = Guid.NewGuid(),
                    PartNumber = request.PartNumber,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddMonths(policy.DurationMonth),
                    PartId = part.PartId,
                    ClaimId = claim.ClaimId,
                    
                };

                await _unitOfWork.Context.PartItems.AddAsync(partItem);

                // Save changes (already handled by ExecuteInTransactionAsync)
                await _unitOfWork.SaveChangesAsync();

                // Return response
                return await GetClaimByIdAsync(claim.ClaimId);
            });
        }

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
