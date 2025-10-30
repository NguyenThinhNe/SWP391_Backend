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
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.BLL.Services.Implements
{
    public class PartItemService : IPartItemService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IMapper _mapper;

        public PartItemService(
            IUnitOfWork<WarrantyDbContext> unitOfWork,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PartItemDto>> GetPartItemsAsync(PartItemDto dto)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();

            // Tạo query không có filter
            var query = repository.CreateBaseQuery(
                include: q => q
                    .Include(p => p.Part)
                    .Include(p => p.Inventory)
                        .ThenInclude(i => i.ServiceCenter)
                    .Include(p => p.ClaimDetails)
                        .ThenInclude(cd => cd.WarrantyClaim),
                asNoTracking: true
            );

            // Đếm tổng số records
            var totalCount = await query.CountAsync();

            // Apply ordering
            query = query.OrderByDescending(p => p.StartDate);

            // Apply pagination
            var items = await query
                .ToListAsync();

            var itemDtos = _mapper.Map<IEnumerable<PartItemDto>>(items);

            return itemDtos;
        }

        public async Task<PartItemDetailDto> GetPartItemByIdAsync(Guid partItemId)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();

            var partItem = await repository.FirstOrDefaultAsync(
                predicate: p => p.PartItemId == partItemId,
                include: q => q
                    .Include(p => p.Part)
                    .Include(p => p.Inventory)
                        .ThenInclude(i => i.ServiceCenter)
                    .Include(p => p.ClaimDetails)
                        .ThenInclude(cd => cd.WarrantyClaim)
            );

            if (partItem == null)
                throw new KeyNotFoundException($"PartItem with ID {partItemId} not found.");

            return _mapper.Map<PartItemDetailDto>(partItem);
        }

        public async Task<PartItemDto> CreatePartItemAsync(CreatePartItemDto createDto)
        {
            // Validate Part exists
            var partRepository = _unitOfWork.GetRepository<Part>();
            var partExists = await partRepository.CountAsync(p => p.PartId == createDto.PartId) > 0;
            if (!partExists)
                throw new KeyNotFoundException($"Part with ID {createDto.PartId} not found.");

            // Validate Inventory exists if provided
            if (createDto.InventoryId.HasValue)
            {
                var inventoryRepository = _unitOfWork.GetRepository<Inventory>();
                var inventoryExists = await inventoryRepository.CountAsync(i => i.InventoryId == createDto.InventoryId.Value) > 0;
                if (!inventoryExists)
                    throw new KeyNotFoundException($"Inventory with ID {createDto.InventoryId} not found.");
            }

            // Validate PartNumber is unique (only if provided)
            if (!string.IsNullOrWhiteSpace(createDto.PartNumber) && await PartNumberExistsAsync(createDto.PartNumber))
                throw new InvalidOperationException($"PartNumber '{createDto.PartNumber}' already exists.");

            // Validate dates
            if (createDto.EndDate <= createDto.StartDate)
                throw new InvalidOperationException("End date must be after start date.");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var partItem = _mapper.Map<PartItem>(createDto);
                partItem.PartItemId = Guid.NewGuid();

                var repository = _unitOfWork.GetRepository<PartItem>();
                await repository.InsertAsync(partItem);
                await _unitOfWork.SaveChangesAsync();

                // Load related entities
                var createdPartItem = await repository.FirstOrDefaultAsync(
                    predicate: p => p.PartItemId == partItem.PartItemId,
                    include: q => q
                        .Include(p => p.Part)
                        .Include(p => p.Inventory)
                        .Include(p => p.ClaimDetails)
                );

                return _mapper.Map<PartItemDto>(createdPartItem);
            });
        }

        public async Task<PartItemDto> UpdatePartItemAsync(Guid partItemId, UpdatePartItemDto updateDto)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();

            var existingPartItem = await repository.FirstOrDefaultAsync(
                predicate: p => p.PartItemId == partItemId
            );

            if (existingPartItem == null)
                throw new KeyNotFoundException($"PartItem with ID {partItemId} not found.");

            // Validate Part exists
            var partRepository = _unitOfWork.GetRepository<Part>();
            var partExists = await partRepository.CountAsync(p => p.PartId == updateDto.PartId) > 0;
            if (!partExists)
                throw new KeyNotFoundException($"Part with ID {updateDto.PartId} not found.");

            // Validate Inventory exists if provided
            if (updateDto.InventoryId.HasValue)
            {
                var inventoryRepository = _unitOfWork.GetRepository<Inventory>();
                var inventoryExists = await inventoryRepository.CountAsync(i => i.InventoryId == updateDto.InventoryId.Value) > 0;
                if (!inventoryExists)
                    throw new KeyNotFoundException($"Inventory with ID {updateDto.InventoryId} not found.");
            }

            // Validate PartNumber is unique (excluding current item, only if provided)
            if (!string.IsNullOrWhiteSpace(updateDto.PartNumber) && await PartNumberExistsAsync(updateDto.PartNumber, partItemId))
                throw new InvalidOperationException($"PartNumber '{updateDto.PartNumber}' already exists.");

            // Validate dates
            if (updateDto.EndDate <= updateDto.StartDate)
                throw new InvalidOperationException("End date must be after start date.");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                _mapper.Map(updateDto, existingPartItem);
                repository.UpdateAsync(existingPartItem);
                await _unitOfWork.SaveChangesAsync();

                // Load related entities
                var updatedPartItem = await repository.FirstOrDefaultAsync(
                    predicate: p => p.PartItemId == partItemId,
                    include: q => q
                        .Include(p => p.Part)
                        .Include(p => p.Inventory)
                        .Include(p => p.ClaimDetails)
                );

                return _mapper.Map<PartItemDto>(updatedPartItem);
            });
        }

        public async Task<bool> DeletePartItemAsync(Guid partItemId)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();

            var partItem = await repository.FirstOrDefaultAsync(
                predicate: p => p.PartItemId == partItemId,
                include: q => q.Include(p => p.ClaimDetails)
            );

            if (partItem == null)
                throw new KeyNotFoundException($"PartItem with ID {partItemId} not found.");

            // Check if PartItem is used in any claims
            if (partItem.ClaimDetails != null && partItem.ClaimDetails.Any())
                throw new InvalidOperationException("Cannot delete PartItem that is used in claims.");

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                repository.DeleteAsync(partItem);
                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }

        public async Task<IEnumerable<PartItemDto>> GetPartItemsByPartIdAsync(Guid partId)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();

            var partItems = await repository.GetListAsync(
                predicate: p => p.PartId == partId,
                include: q => q
                    .Include(p => p.Part)
                    .Include(p => p.Inventory)
                    .Include(p => p.ClaimDetails)
                        .ThenInclude(cd => cd.WarrantyClaim),
                orderBy: q => q.OrderByDescending(p => p.StartDate)
            );

            return _mapper.Map<IEnumerable<PartItemDto>>(partItems);
        }

        public async Task<IEnumerable<PartItemDto>> GetPartItemsByInventoryIdAsync(Guid inventoryId)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();

            var partItems = await repository.GetListAsync(
                predicate: p => p.InventoryId == inventoryId,
                include: q => q
                    .Include(p => p.Part)
                    .Include(p => p.Inventory)
                    .Include(p => p.ClaimDetails)
                        .ThenInclude(cd => cd.WarrantyClaim),
                orderBy: q => q.OrderByDescending(p => p.StartDate)
            );

            return _mapper.Map<IEnumerable<PartItemDto>>(partItems);
        }

        public async Task<IEnumerable<PartItemDto>> GetPartItemsByClaimIdAsync(Guid claimId)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();

            var partItems = await repository.GetListAsync(
                predicate: p => p.ClaimDetails.Any(cd => cd.ClaimId == claimId),
                include: q => q
                    .Include(p => p.Part)
                    .Include(p => p.Inventory)
                    .Include(p => p.ClaimDetails)
                        .ThenInclude(cd => cd.WarrantyClaim),
                orderBy: q => q.OrderByDescending(p => p.StartDate)
            );

            return _mapper.Map<IEnumerable<PartItemDto>>(partItems);
        }

        public async Task<bool> PartItemExistsAsync(Guid partItemId)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();
            return await repository.CountAsync(p => p.PartItemId == partItemId) > 0;
        }

        public async Task<bool> PartNumberExistsAsync(string partNumber, Guid? excludePartItemId = null)
        {
            var repository = _unitOfWork.GetRepository<PartItem>();

            if (excludePartItemId.HasValue)
            {
                return await repository.CountAsync(
                    p => p.PartNumber == partNumber && p.PartItemId != excludePartItemId.Value
                ) > 0;
            }

            return await repository.CountAsync(p => p.PartNumber == partNumber) > 0;
        }

        #region Private Methods

        private System.Linq.Expressions.Expression<Func<PartItem, bool>> BuildFilterPredicate(PartItemFilterDto filter)
        {
            System.Linq.Expressions.Expression<Func<PartItem, bool>> predicate = p => true;

            if (!string.IsNullOrWhiteSpace(filter.PartNumber))
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.PartNumber.Contains(filter.PartNumber);
            }

            if (filter.PartId.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.PartId == filter.PartId.Value;
            }

            if (filter.InventoryId.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.InventoryId == filter.InventoryId.Value;
            }

            if (filter.ClaimId.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.ClaimDetails.Any(cd => cd.ClaimId == filter.ClaimId.Value);
            }

            if (filter.StartDateFrom.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.StartDate >= filter.StartDateFrom.Value;
            }

            if (filter.StartDateTo.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.StartDate <= filter.StartDateTo.Value;
            }

            if (filter.EndDateFrom.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.EndDate >= filter.EndDateFrom.Value;
            }

            if (filter.EndDateTo.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.EndDate <= filter.EndDateTo.Value;
            }

            if (filter.MinQuantity.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.Quantity >= filter.MinQuantity.Value;
            }

            if (filter.MaxQuantity.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.Quantity <= filter.MaxQuantity.Value;
            }

            if (filter.MinPrice.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.Price >= filter.MinPrice.Value;
            }

            if (filter.MaxPrice.HasValue)
            {
                var previous = predicate;
                predicate = p => previous.Compile()(p) && p.Price <= filter.MaxPrice.Value;
            }

            if (filter.IsInInventory.HasValue)
            {
                var previous = predicate;
                if (filter.IsInInventory.Value)
                {
                    predicate = p => previous.Compile()(p) && p.InventoryId.HasValue;
                }
                else
                {
                    predicate = p => previous.Compile()(p) && !p.InventoryId.HasValue;
                }
            }

            return predicate;
        }

        #endregion
    }
}
