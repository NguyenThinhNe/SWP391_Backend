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
        private readonly IPartItemService _partItemService;
        private readonly IWorkOrderService _workOrderService;
        public ClaimService(IUnitOfWork<WarrantyDbContext> unitOfWork, IMapper mapper, IPartItemService partItemService, IWorkOrderService workOrderService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _partItemService = partItemService;
            _workOrderService = workOrderService;
        }
        #region Create warranty claim 
        public async Task<ClaimResponse> CreateClaimAsync(ClaimRequest request, Guid currentUserId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
                var vehicleRepo = _unitOfWork.GetRepository<CustomerVehicle>();
                var partRepo = _unitOfWork.GetRepository<Part>();
                var partItemRepo = _unitOfWork.GetRepository<PartItem>();
                var claimDetailRepo = _unitOfWork.GetRepository<ClaimDetail>();
                var claimImageRepo = _unitOfWork.GetRepository<ClaimImage>();
                var userRepo = _unitOfWork.GetRepository<User>();
                // ✅ 2. Validate User và Service Center
                var user = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserId == currentUserId,
                    include: q => q.Include(u => u.ServiceCenter)
                );

                if (user == null)
                    throw new UnauthorizedAccessException("User not found");

                if (user.ServiceCenterId == null || user.ServiceCenter == null)
                    throw new InvalidOperationException("User must belong to a Service Center to create claims");
                var vehicle = await vehicleRepo.FirstOrDefaultAsync(predicate: x => x.VIN == request.VIN);
                if (vehicle == null)
                    throw new Exception($"Vehicle with VIN {request.VIN} not found.");

               
                var claim = new WarrantyClaim
                {
                    ClaimId = Guid.NewGuid(),
                    ClaimDate = request.ClaimDate,
                    VIN = request.VIN,
                    IssueDescription = request.IssueDescription,
                    Status = WarrantyClaimStatus.Pending,
                    UserId = currentUserId,
                    isActive = true,
                    
                };

                await claimRepo.InsertAsync(claim);


                foreach (var item in request.PartItems)
                {
                    ClaimDetail detail = new ClaimDetail
                    {
                        ClaimDetailId = Guid.NewGuid(),
                        ClaimId = claim.ClaimId,
                        PartItemId = null,
                        ActionType = request.ActionType
                    };


                    await _partItemService.HandleClaimPartItemsAsync(request, claim);
                    await claimDetailRepo.InsertAsync(detail);
                }
                return _mapper.Map<ClaimResponse>(claim);
            });
        }




        #endregion
        public async Task<ClaimResponse> UpdateClaimAsync(Guid claimId, UpdateClaimPartItemsRequest request)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
                var claimDetailRepo = _unitOfWork.GetRepository<ClaimDetail>();

                // ✅ 1: Load claim including ClaimDetails
                var claim = await claimRepo.FirstOrDefaultAsync(
                    predicate: c => c.ClaimId == claimId && c.Status == WarrantyClaimStatus.Pending,
                    include: q => q.Include(c => c.ClaimDetails)
                );

                if (claim == null)
                    throw new KeyNotFoundException($"Claim with ID {claimId} not found.");

                // ✅ 2: VIN cannot change (validation for safety)
                if (request.VIN != claim.VIN)
                    throw new InvalidOperationException("VIN cannot be modified for an existing claim.");

                // ✅ 3: Remove all existing ClaimDetails (we will recreate)
                if (claim.ClaimDetails.Any())
                    claimDetailRepo.DeleteRangeAsync(claim.ClaimDetails);

                // ✅ 4: Reprocess all PartItems
                var tempRequest = new ClaimRequest
                {
                    VIN = claim.VIN,
                    ClaimDate = claim.ClaimDate,
                    IssueDescription = request.IssueDescription,
                    ActionType = request.ActionType,
                    PartItems = request.PartItems
                };

                await _partItemService.HandleClaimPartItemsAsync(tempRequest, claim);

                await _unitOfWork.SaveChangesAsync();

                // ✅ 5: Return updated claim
                return await GetClaimByIdAsync(claimId);
            });
        }



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
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
                var claimDetailRepo = _unitOfWork.GetRepository<ClaimDetail>();
                var partRepo = _unitOfWork.GetRepository<Part>();
                var partItemRepo = _unitOfWork.GetRepository<PartItem>();

                // Lấy claim bằng repository và include các navigation cần thiết
                var claim = await claimRepo.FirstOrDefaultAsync(
                    predicate: c => c.ClaimId == claimId,
                    include: q => q
                        .Include(c => c.CustomerVehicle)
                        .Include(c => c.ClaimDetails)           // cần claim details
                            .ThenInclude(cd => cd.PartItem)     // nếu đã có PartItem nav
                                .ThenInclude(pi => pi.Part)
                        .Include(c => c.WarrantyPolicy)
                        .Include(c => c.User)
                            .ThenInclude(u => u.ServiceCenter)
                );

                if (claim == null)
                    throw new KeyNotFoundException($"Claim with ID {claimId} not found");

                // Chỉ cho phép approve từ trạng thái Pending (theo validate chuyển trạng thái của bạn)
                if (claim.Status != WarrantyClaimStatus.Pending)
                    throw new InvalidOperationException($"Cannot approve claim with status {claim.Status}. Only Pending claims can be approved.");


                //// ✅ Tạo PartItem nếu cần (tùy theo ActionType)
                //foreach (var detail in claim.ClaimDetails)
                //{
                //    if (detail.ActionType == ClaimActionType.Replacement ||
                //        detail.ActionType == ClaimActionType.ProvidedPart)
                //    {
                        
                //        var part = await partRepo.FirstOrDefaultAsync(p => p.PartName == detail.PartItem?.Part?.PartName);
                //        if (part == null) continue;

                //        var newPartItem = new PartItem
                //        {
                //            PartItemId = Guid.NewGuid(),
                //            PartId = part.PartId,
                //            PartNumber = Guid.NewGuid().ToString("N").Substring(0, 8),
                //            StartDate = DateTime.Now,
                //            EndDate = DateTime.Now.AddMonths(12),
                //            Quantity = 1,
                //            Price = 0
                //        };

                //        await partItemRepo.InsertAsync(newPartItem);
                //        detail.PartItemId = newPartItem.PartItemId;
                //        claimDetailRepo.UpdateAsync(detail);
                //    }
                //}

                // ✅ Cập nhật trạng thái
                claim.Status = WarrantyClaimStatus.Accepted;
                claimRepo.UpdateAsync(claim);
                await _unitOfWork.SaveChangesAsync();
                return await GetClaimByIdAsync(claimId);
            });
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
                if (newStatus == WarrantyClaimStatus.Accepted)
                {
                    var workOrderRequest = new WorkOrderRequest
                    {
                        ClaimId = claim.ClaimId,
                       
                    };

                    // Gọi WorkOrderService để tạo work order
                    await _workOrderService.CreateWorkOrderAsync(workOrderRequest);
                }
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
          private ClaimRequest ConvertToClaimRequest(UpdateClaimPartItemsRequest request)
        {
            return new ClaimRequest
            {
                
                VIN = request.VIN,
                
                PartItems = request.PartItems,
                ActionType = request.ActionType,
            };
        }
    }
 

    }
