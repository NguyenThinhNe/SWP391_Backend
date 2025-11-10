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
using WarrantyManagement.DAL.Data.Response;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.BLL.Services.Implements
{
    public class WarrantyValidationService : IWarrantyValidationService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IMapper _mapper;

        public WarrantyValidationService(
            IUnitOfWork<WarrantyDbContext> unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<WarrantyValidationDto> ValidatePolicyForPartItemAsync(Guid policyId, Guid partItemId)
        {
            // Get repositories from UnitOfWork
            var policyRepo = _unitOfWork.GetRepository<WarrantyPolicy>();
            var partItemRepo = _unitOfWork.GetRepository<PartItem>();

            // Fetch policy
            var policy = await policyRepo.FirstOrDefaultAsync(
                predicate: p => p.PolicyId == policyId
            );

            if (policy == null)
            {
                return new WarrantyValidationDto
                {
                    IsValid = false,
                    Message = "Warranty policy not found"
                };
            }

            // Fetch part item with includes
            var partItem = await partItemRepo.FirstOrDefaultAsync(
                predicate: p => p.PartItemId == partItemId,
                include: q => q.Include(p => p.Part)
            );

            if (partItem == null)
            {
                return new WarrantyValidationDto
                {
                    IsValid = false,
                    Message = "Part item not found"
                };
            }

            return ValidateWarranty(policy, partItem);
        }

        public async Task<WarrantyValidationDto> CheckWarrantyStatusAsync(Guid partItemId)
        {
            var partItemRepo = _unitOfWork.GetRepository<PartItem>();

            // Fetch part item with all related data
            var partItem = await partItemRepo.FirstOrDefaultAsync(
                predicate: p => p.PartItemId == partItemId,
                include: q => q
                    .Include(p => p.Part)
                    .Include(p => p.ClaimDetails)
                        .ThenInclude(cd => cd.WarrantyClaim)
                        .ThenInclude(wc => wc.WarrantyPolicy)
            );

            if (partItem == null)
            {
                return new WarrantyValidationDto
                {
                    IsValid = false,
                    Message = "Part item not found"
                };
            }

            // Lấy policy từ claim detail gần nhất
            var policy = partItem.ClaimDetails?
                .Where(cd => cd.WarrantyClaim?.WarrantyPolicy != null)
                .Select(cd => cd.WarrantyClaim.WarrantyPolicy)
                .OrderByDescending(p => p.CreateTime)
                .FirstOrDefault();

            if (policy == null)
            {
                return new WarrantyValidationDto
                {
                    IsValid = false,
                    Message = "No warranty policy associated with this part item",
                    PartItem = _mapper.Map<PartItemDto>(partItem)
                };
            }

            return ValidateWarranty(policy, partItem);
        }

        public async Task<IEnumerable<WarrantyValidationDto>> CheckMultiplePartItemsAsync(IEnumerable<Guid> partItemIds)
        {
            var partItemRepo = _unitOfWork.GetRepository<PartItem>();

            // Fetch multiple part items at once
            var partItems = await partItemRepo.GetListAsync(
                predicate: p => partItemIds.Contains(p.PartItemId),
                include: q => q
                    .Include(p => p.Part)
                    .Include(p => p.ClaimDetails)
                        .ThenInclude(cd => cd.WarrantyClaim)
                        .ThenInclude(wc => wc.WarrantyPolicy)
            );

            var results = new List<WarrantyValidationDto>();

            foreach (var partItem in partItems)
            {
                var policy = partItem.ClaimDetails?
                    .Where(cd => cd.WarrantyClaim?.WarrantyPolicy != null)
                    .Select(cd => cd.WarrantyClaim.WarrantyPolicy)
                    .OrderByDescending(p => p.CreateTime)
                    .FirstOrDefault();

                if (policy == null)
                {
                    results.Add(new WarrantyValidationDto
                    {
                        IsValid = false,
                        Message = "No warranty policy associated with this part item",
                        PartItem = _mapper.Map<PartItemDto>(partItem)
                    });
                }
                else
                {
                    results.Add(ValidateWarranty(policy, partItem));
                }
            }

            return results;
        }
        public async Task<IEnumerable<WarrantyValidationDto>> GetExpiringWarrantiesAsync(int daysThreshold = 30)
        {
            var partItemRepo = _unitOfWork.GetRepository<PartItem>();

            // Lấy tất cả part items có thể có warranty sắp hết hạn
            var partItems = await partItemRepo.GetListAsync(
                predicate: p => p.Status == PartItemStatus.Available,
                include: q => q
                    .Include(p => p.Part)
                    .Include(p => p.ClaimDetails)
                        .ThenInclude(cd => cd.WarrantyClaim)
                        .ThenInclude(wc => wc.WarrantyPolicy)
            );

            var results = new List<WarrantyValidationDto>();

            foreach (var partItem in partItems)
            {
                var policy = partItem.ClaimDetails?
                    .Where(cd => cd.WarrantyClaim?.WarrantyPolicy != null)
                    .Select(cd => cd.WarrantyClaim.WarrantyPolicy)
                    .OrderByDescending(p => p.CreateTime)
                    .FirstOrDefault();

                if (policy != null)
                {
                    var validation = ValidateWarranty(policy, partItem);

                    // Chỉ thêm những warranty còn hiệu lực nhưng sắp hết hạn
                    if (validation.IsValid &&
                        validation.RemainingDays.HasValue &&
                        validation.RemainingDays.Value <= daysThreshold)
                    {
                        results.Add(validation);
                    }
                }
            }

            return results.OrderBy(r => r.RemainingDays);
        }
        private WarrantyValidationDto ValidateWarranty(WarrantyPolicy policy, PartItem partItem)
        {
            var validationResult = new WarrantyValidationDto
            {
                Policy = _mapper.Map<WarrantyPolicyDto>(policy),
                PartItem = _mapper.Map<PartItemDto>(partItem),
                WarrantyStartDate = partItem.StartDate
            };

            // 1. Kiểm tra status của part item
            if (partItem.Status != PartItemStatus.Available)
            {
                validationResult.IsValid = false;
                validationResult.Message = $"Part item status is '{partItem.Status}'. Warranty only applies to available items.";
                return validationResult;
            }

            // 2. Tính ngày hết hạn bảo hành
            var warrantyEndDate = partItem.StartDate.AddMonths(policy.DurationMonth);
            validationResult.WarrantyEndDate = warrantyEndDate;

            // 3. Kiểm tra ngày bắt đầu
            if (DateTime.Now < partItem.StartDate)
            {
                validationResult.IsValid = false;
                validationResult.Message = $"Warranty has not started yet. Start date: {partItem.StartDate:dd/MM/yyyy}";
                return validationResult;
            }

            // 4. Kiểm tra đã hết hạn chưa
            if (DateTime.Now > warrantyEndDate)
            {
                var expiredDays = (DateTime.Now - warrantyEndDate).Days;
                validationResult.IsValid = false;
                validationResult.Message = $"Warranty expired {expiredDays} days ago on {warrantyEndDate:dd/MM/yyyy}";
                validationResult.RemainingDays = 0;
                return validationResult;
            }

            // 5. Warranty còn hiệu lực
            var remainingDays = (warrantyEndDate - DateTime.Now).Days;
            validationResult.IsValid = true;
            validationResult.RemainingDays = remainingDays;

            if (remainingDays <= 30)
            {
                validationResult.Message = $"⚠️ Warranty is expiring soon! Only {remainingDays} days remaining until {warrantyEndDate:dd/MM/yyyy}";
            }
            else
            {
                validationResult.Message = $"✓ Warranty is active. {remainingDays} days remaining until {warrantyEndDate:dd/MM/yyyy}";
            }

            return validationResult;
        }
    }
}



