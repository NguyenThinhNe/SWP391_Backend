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
    public class ClaimService : IClaimService
    {
       private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
       private readonly IMapper _mapper;
    
       public ClaimService(IUnitOfWork<WarrantyDbContext> unitOfWork, IMapper mapper)
       {
           _unitOfWork = unitOfWork;
           _mapper = mapper;
       }

        #region Create warranty claim 
        public async Task<ClaimResponse> CreateClaimAsync(ClaimRequest request, Guid technicianId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Get repositories
                var vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();
                var policyRepo = _unitOfWork.GetRepository<WarrantyPolicy>();
                var userRepo = _unitOfWork.GetRepository<User>();
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
                var partRepo = _unitOfWork.GetRepository<Part>();
                var partItemRepo = _unitOfWork.GetRepository<PartItem>();

                // 1. Validate warranty eligibility
                var isEligible = await ValidateWarrantyEligibilityAsync(request.VIN, request.PolicyId);
                if (!isEligible)
                {
                    throw new InvalidOperationException("Vehicle is not eligible for warranty claim");
                }

                // 2. Get and validate vehicle
                var vehicle = await vehicleRepo.FirstOrDefaultAsync(
                    predicate: v => v.VIN == request.VIN
                );

                if (vehicle == null)
                {
                    throw new KeyNotFoundException($"Vehicle with VIN '{request.VIN}' not found");
                }

                // 3. Validate policy exists
                var policy = await policyRepo.FirstOrDefaultAsync(
                    predicate: p => p.PolicyId == request.PolicyId
                );

                if (policy == null)
                {
                    throw new KeyNotFoundException($"Warranty policy with ID '{request.PolicyId}' not found");
                }

                // 4. Validate user exists and is technician
                var user = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserId == technicianId
                );

                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID '{technicianId}' not found");
                }

                // 5. Update vehicle information from request
                vehicle.VehicleName = request.VehicleName;
                vehicle.PurchaseDate = request.PurchaseDate;
                vehicle.MileAge = request.Mileage;
                vehicleRepo.UpdateAsync(vehicle);

                // 6. Map ClaimRequest to WarrantyClaim
                var claim = _mapper.Map<WarrantyClaim>(request);
                claim.ClaimId = Guid.NewGuid();
                claim.UserId = technicianId;
                claim.Status = WarrantyClaimStatus.Pending;

                // 7. Add the claim to database
                await claimRepo.InsertAsync(claim);

                // ✅ 8. Process ALL parts from request
                foreach (var partItemRequest in request.PartItems)
                {
                    // 8.1 Validate part exists
                    var existingPart = await partRepo.FirstOrDefaultAsync(
                        predicate: p => p.PartId == partItemRequest.PartId
                    );

                    if (existingPart == null)
                    {
                        throw new KeyNotFoundException(
                            $"Part with ID '{partItemRequest.PartId}' not found");
                    }

                    // 8.2 Create PartItem linking the claim to the part
                    var partItem = new PartItem
                    {
                        PartItemId = Guid.NewGuid(),
                        ClaimId = claim.ClaimId,
                        PartId = partItemRequest.PartId,
                        PartNumber = partItemRequest.PartNumber,
                        Quantity = partItemRequest.Quantity, // ✅ Lấy đúng quantity
                    };

                    await partItemRepo.InsertAsync(partItem);
                }

                // 9. Save changes
                await _unitOfWork.SaveChangesAsync();

                // 10. Retrieve the complete claim with all details
                return await GetClaimByIdAsync(claim.ClaimId);
            });
        }
        #endregion

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

        public async Task<ICollection<ClaimResponse>> GetClaimsAsync(Guid? serviceCenterId = null)
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

            var claims = await query
                .OrderByDescending(c => c.ClaimDate)
                .ToListAsync();

            return _mapper.Map<ICollection<ClaimResponse>>(claims);
        }

        public async Task<ICollection<ClaimResponse>> GetClaimsByServiceCenterAsync(Guid serviceCenterId)
        {
            var serviceCenterRepo = _unitOfWork.GetRepository<ServiceCenter>();
            var serviceCenterExists = await serviceCenterRepo.CountAsync(
                predicate: sc => sc.CenterId == serviceCenterId
            ) > 0;

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
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();

                var claim = await claimRepo.FirstOrDefaultAsync(
                    predicate: c => c.ClaimId == claimId
                );

                if (claim == null)
                {
                    throw new KeyNotFoundException($"Claim with ID {claimId} not found");
                }

                ValidateStatusTransition(claim.Status, WarrantyClaimStatus.Completed);

                claim.Status = WarrantyClaimStatus.Completed;

                claimRepo.UpdateAsync(claim);
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
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();

                var claim = await claimRepo.FirstOrDefaultAsync(
                    predicate: c => c.ClaimId == claimId
                );

                if (claim == null)
                {
                    throw new KeyNotFoundException($"Claim with ID {claimId} not found");
                }

                ValidateStatusTransition(claim.Status, newStatus);

                claim.Status = newStatus;

                claimRepo.UpdateAsync(claim);
                await _unitOfWork.SaveChangesAsync();

                return await GetClaimByIdAsync(claimId);
            });
        }

        #endregion

        #region Validation

        public async Task<bool> ValidateWarrantyEligibilityAsync(string vin, Guid policyId)
        {
            var vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();
            var policyRepo = _unitOfWork.GetRepository<WarrantyPolicy>();

            var vehicle = await vehicleRepo.FirstOrDefaultAsync(
                predicate: v => v.VIN == vin
            );

            if (vehicle == null)
            {
                return false;
            }

            var policy = await policyRepo.FirstOrDefaultAsync(
                predicate: p => p.PolicyId == policyId
            );

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
