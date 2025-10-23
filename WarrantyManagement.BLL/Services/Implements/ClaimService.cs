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
            Guid newClaimId = Guid.Empty;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Repositories
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
                var partRepo = _unitOfWork.GetRepository<Part>();
                var partItemRepo = _unitOfWork.GetRepository<PartItem>();
                var vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();
                var policyRepo = _unitOfWork.GetRepository<WarrantyPolicy>();
                var userRepo = _unitOfWork.GetRepository<User>();

                // ✅ 1. Lấy thông tin vehicle theo VIN
                var vehicle = await vehicleRepo.FirstOrDefaultAsync(
                    predicate: v => v.VIN == request.VIN
                );

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle with VIN '{request.VIN}' not found");

                // ✅ 2. Lấy policy
                var policy = await policyRepo.FirstOrDefaultAsync(
                    predicate: p => p.PolicyId == request.PolicyId
                );

                if (policy == null)
                    throw new KeyNotFoundException($"Warranty policy with ID '{request.PolicyId}' not found");

                // ✅ 3. Lấy technician (user)
                var user = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserId == technicianId
                );

                if (user == null)
                    throw new KeyNotFoundException($"User with ID '{technicianId}' not found");

                // ✅ 4. Cập nhật lại thông tin vehicle (nếu có)
                vehicle.VehicleName = request.VehicleName;
                vehicle.PurchaseDate = request.PurchaseDate;
                vehicle.MileAge = request.Mileage;
                vehicleRepo.UpdateAsync(vehicle);

                // ✅ 5. Map ClaimRequest → WarrantyClaim
                var claim = _mapper.Map<WarrantyClaim>(request);
                claim.ClaimId = Guid.NewGuid();
                claim.UserId = technicianId;
                claim.Status = WarrantyClaimStatus.Pending;

                // ✅ 6. Lưu Claim vào DB
                await claimRepo.InsertAsync(claim);
                newClaimId = claim.ClaimId;

                // ✅ 7. Thêm danh sách PartItem
                foreach (var item in request.PartItems)
                {
                    // Kiểm tra Part tồn tại
                    var part = await partRepo.FirstOrDefaultAsync(
                        predicate: p => p.PartId == item.PartId
                    );

                    if (part == null)
                        throw new KeyNotFoundException($"Part with ID '{item.PartId}' not found");
                    // Tạo PartItem mới
                    var partItem = new PartItem
                    {
                        PartItemId = Guid.NewGuid(),
                        ClaimId = claim.ClaimId,
                        PartId = part.PartId,
                        PartNumber = item.PartNumber,
                        
                        Quantity = item.Quantity
                    };

                    await partItemRepo.InsertAsync(partItem);
                }

                // ✅ Lưu thay đổi trong transaction
                await _unitOfWork.SaveChangesAsync();
            });

            // ✅ Transaction đã commit
            return await GetClaimByIdAsync(newClaimId);
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
