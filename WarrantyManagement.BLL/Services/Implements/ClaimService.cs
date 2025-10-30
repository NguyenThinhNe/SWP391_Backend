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
                var claimDetailRepo = _unitOfWork.GetRepository<ClaimDetail>();
                var vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();
                var policyRepo = _unitOfWork.GetRepository<WarrantyPolicy>();
                var userRepo = _unitOfWork.GetRepository<User>();

                // ✅ 1. Kiểm tra vehicle tồn tại theo VIN
                var vehicle = await vehicleRepo.FirstOrDefaultAsync(
                    predicate: v => v.VIN == request.VIN
                );

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle with VIN '{request.VIN}' not found");

                // ✅ 2. Kiểm tra policy tồn tại
                var policy = await policyRepo.FirstOrDefaultAsync(
                    predicate: p => p.PolicyId == request.PolicyId
                );

                if (policy == null)
                    throw new KeyNotFoundException($"Warranty policy with ID '{request.PolicyId}' not found");

                // ✅ 3. Kiểm tra technician (user) tồn tại
                var user = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserId == technicianId
                );

                if (user == null)
                    throw new KeyNotFoundException($"User with ID '{technicianId}' not found");

                // ✅ 4. Map ClaimRequest → WarrantyClaim
                var claim = _mapper.Map<WarrantyClaim>(request);
                claim.ClaimId = Guid.NewGuid();
                claim.UserId = technicianId;
                claim.Status = WarrantyClaimStatus.Pending;

                // ✅ 5. Lưu Claim vào DB
                await claimRepo.InsertAsync(claim);
                newClaimId = claim.ClaimId;

                // ✅ 6. Xử lý PartItems và tạo ClaimDetails (many-to-many relationship)
                foreach (var partItemRequest in request.PartItems)
                {
                    // Kiểm tra Part tồn tại
                    var part = await partRepo.FirstOrDefaultAsync(
                        predicate: p => p.PartId == partItemRequest.PartId
                    );

                    if (part == null)
                        throw new KeyNotFoundException($"Part with ID '{partItemRequest.PartId}' not found");

                    // Map PartItemRequest → PartItem
                    var partItem = _mapper.Map<PartItem>(partItemRequest);
                    partItem.PartItemId = Guid.NewGuid();

                    // Lưu PartItem
                    await partItemRepo.InsertAsync(partItem);

                    // Tạo ClaimDetail để liên kết Claim và PartItem (many-to-many)
                    var claimDetail = new ClaimDetail
                    {
                        ClaimDetailId = Guid.NewGuid(),
                        ClaimId = claim.ClaimId,
                        PartItemId = partItem.PartItemId
                    };

                    await claimDetailRepo.InsertAsync(claimDetail);
                }

                // ✅ 7. Lưu thay đổi trong transaction
                await _unitOfWork.SaveChangesAsync();
            });

            // ✅ Transaction đã commit, lấy claim với đầy đủ thông tin
            return await GetClaimByIdAsync(newClaimId);
        }

        #endregion
        //#region Update Claim
        //public async Task<ClaimResponse> UpdateClaimAsync(UpdateClaimRequest updateRequest, Guid claimId)
        //{
        //    var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
        //    var claimDetailRepo = _unitOfWork.GetRepository<ClaimDetail>();

        //    // Include current ClaimDetails so we can remove and re-add them
        //    var existingClaim = await claimRepo.FirstOrDefaultAsync(
        //        predicate: c => c.ClaimId == claimId,
        //        include: query => query.Include(c => c.ClaimDetails)
        //                               .ThenInclude(cd => cd.PartItem)
        //                               .Include(c => c.WarrantyPolicy)
        //                               .Include(c => c.User)
        //                               .Include(c => c.CustomerVehicle)
        //    );

        //    if (existingClaim == null)
        //        throw new Exception($"Claim with ID {claimId} not found");

        //    // Map updated fields
        //    _mapper.Map(updateRequest, existingClaim);

        //    // Remove existing claim details
        //    if (existingClaim.ClaimDetails != null && existingClaim.ClaimDetails.Any())
        //    {
        //        claimDetailRepo.DeleteRangeAsync(existingClaim.ClaimDetails);
        //    }

        //    // Add updated claim details
        //    existingClaim.ClaimDetails = updateRequest.PartItems.Select(p => new ClaimDetail
        //    {
        //        ClaimDetailId = Guid.NewGuid(),
        //        ClaimId = existingClaim.ClaimId,
        //        PartItemId = p.PartId
        //    }).ToList();

        //    // Update in DB
        //    claimRepo.UpdateAsync(existingClaim);
        //    await _unitOfWork.SaveChangesAsync();

        //    // Map response
        //    var response = new ClaimResponse
        //    {
        //        ClaimId = existingClaim.ClaimId,
        //        ClaimDate = existingClaim.ClaimDate,
        //        VIN = existingClaim.VIN,
        //        ClaimStatus = existingClaim.Status,
        //        IssueDescription = existingClaim.IssueDescription,
        //        ClaimDescription = existingClaim.ClaimDescription,
        //        PolicyId = existingClaim.PolicyId,
        //        PolicyName = existingClaim.WarrantyPolicy?.Name,
        //        ServiceCenterId = existingClaim.User?.ServiceCenterId ?? Guid.Empty,
        //        ServiceCenterName = existingClaim.User?.ServiceCenter?.CenterName ?? string.Empty,
        //        UserId = existingClaim.UserId,
        //        TechnicianName = existingClaim.User?.Name,
        //        VehicleName = existingClaim.CustomerVehicle?.VehicleName ?? string.Empty,
        //        PurchaseDate = existingClaim.CustomerVehicle?.PurchaseDate ?? DateTime.MinValue,
        //        Mileage = existingClaim.CustomerVehicle?.MileAge ?? 0,
        //        Parts = existingClaim.ClaimDetails?.Select(cd => new PartItemResponse
        //        {
        //            PartItemId = cd.PartItemId,
        //            PartId = cd.PartItem.PartId,
        //            PartNumber = cd.PartItem.PartNumber,
        //            PartName = cd.PartItem.Part?.PartName ?? string.Empty,
        //            Description = cd.PartItem.Part?.Description ?? string.Empty,
        //            Price = cd.PartItem.Price,
        //            Quantity = cd.PartItem.Quantity,
        //            TotalCost = cd.PartItem.Price * cd.PartItem.Quantity
        //        }).ToList()
        //    };

        //    return response;
        //}
        //#endregion

        #region Get Claims

        public async Task<ClaimResponse> GetClaimByIdAsync(Guid claimId)
        {
            var claim = await _unitOfWork.Context.WarrantyClaims
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Customer)
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Campaign)
                .Include(c => c.ClaimDetails)
                    .ThenInclude(cd => cd.PartItem)
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
                .Include(c => c.ClaimDetails)
                    .ThenInclude(cd => cd.PartItem)
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
                .Include(c => c.ClaimDetails)
                    .ThenInclude(cd => cd.PartItem)
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

        public async Task<ICollection<ClaimResponse>> GetClaimsByUserAsync(Guid userId)
        {
            var claims = await _unitOfWork.Context.WarrantyClaims
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Customer)
                .Include(c => c.CustomerVehicle)
                    .ThenInclude(v => v.Campaign)
                .Include(c => c.ClaimDetails)
                    .ThenInclude(cd => cd.PartItem)
                        .ThenInclude(pi => pi.Part)
                .Include(c => c.WarrantyPolicy)
                .Include(c => c.User)
                    .ThenInclude(u => u.ServiceCenter)
                .AsNoTracking()
                .Where(c => c.UserId == userId)
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
                .Include(c => c.ClaimDetails)
                    .ThenInclude(cd => cd.PartItem)
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


        public async Task<ClaimResponse> ApproveClaimAsync(Guid claimId, Guid evmStaffId)
        {
            return await UpdateClaimStatusAsync(claimId, WarrantyClaimStatus.Accepted, evmStaffId);
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

                ValidateStatusTransition(claim.Status, WarrantyClaimStatus.Accepted);

                claim.Status = WarrantyClaimStatus.Accepted;
                // TODO: Lưu rejectionReason nếu cần (có thể thêm field vào WarrantyClaim entity)

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

            // Kiểm tra warranty còn hiệu lực
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
                        WarrantyClaimStatus.Accepted,
                        WarrantyClaimStatus.Overdued
                    }
                },
            {
            WarrantyClaimStatus.Accepted,
            new List<WarrantyClaimStatus>
            {
                WarrantyClaimStatus.Overdued
            }
            },
            {
            WarrantyClaimStatus.Rejected,
            new List<WarrantyClaimStatus>()
            },
            {
            WarrantyClaimStatus.Overdued,
            new List<WarrantyClaimStatus>
            {
                WarrantyClaimStatus.Accepted,
                WarrantyClaimStatus.Rejected
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

        #region Delete Claim

        public async Task<bool> DeleteClaimAsync(Guid claimId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();

                // ✅ 1. Tìm claim theo ID
                var claim = await claimRepo.FirstOrDefaultAsync(
                    predicate: c => c.ClaimId == claimId
                );

                if (claim == null)
                {
                    throw new KeyNotFoundException($"Claim with ID {claimId} not found");
                }

                // ✅ 2. Kiểm tra nếu claim đã bị xóa mềm
                if (!claim.isActive)
                {
                    throw new InvalidOperationException($"Claim with ID {claimId} is already deleted.");
                }

                // ✅ 3. Soft delete bằng cách set isActive = false
                claim.isActive = false;

                // ✅ 4. Cập nhật vào DB
                claimRepo.UpdateAsync(claim);
                await _unitOfWork.SaveChangesAsync();

                return true;
            });
        }

        #endregion
    }
}
