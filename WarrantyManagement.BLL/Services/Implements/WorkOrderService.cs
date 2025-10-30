using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public class WorkOrderService  : IWorkOrderService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IMapper _mapper;

        public WorkOrderService(IUnitOfWork<WarrantyDbContext> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        #region Create WorkOrder

        public async Task<WorkOrderResponse> CreateWorkOrderAsync(WorkOrderRequest request)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Validate claim exists and is approved
                var claimRepo = _unitOfWork.GetRepository<WarrantyClaim>();
                var claim = await claimRepo.FirstOrDefaultAsync(
                    predicate: c => c.ClaimId == request.ClaimId,
                    include: null
                );

                if (claim == null)
                    throw new Exception("Claim not found");
                if (claim.Status != WarrantyClaimStatus.Accepted)
                    throw new Exception("Claim must be approved before creating a work order");
                // Validate technician exists
                var userRepo = _unitOfWork.GetRepository<User>();
                var technician = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserId == request.TechnicianId,
                    include: null
                );

                if (technician == null)
                    throw new Exception("Technician not found");

                // Validate customer exists
                var customerRepo = _unitOfWork.GetRepository<Customer>();
                var customer = await customerRepo.FirstOrDefaultAsync(
                    predicate: c => c.CustomerId == request.CustomerId,
                    include: null
                );

                if (customer == null)
                    throw new Exception("Customer not found");

                // Map request to entity
                var workOrder = _mapper.Map<WorkOrder>(request);
                workOrder.WorkOrderId = Guid.NewGuid();

                // Handle Parts if provided
                if (request.PartIds != null && request.PartIds.Any())
                {
                    var partRepo = _unitOfWork.GetRepository<Part>();
                    var parts = await partRepo.GetListAsync(
                        predicate: p => request.PartIds.Contains(p.PartId),
                        include: null
                    );

                    workOrder.Parts = parts.ToList();
                }

                // Insert work order
                var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();
                await workOrderRepo.InsertAsync(workOrder);

                await _unitOfWork.SaveChangesAsync();

                // Return response with full data
                return await GetWorkOrderByIdAsync(workOrder.WorkOrderId);
            });
        }

        #endregion

        #region Get WorkOrder

        public async Task<WorkOrderResponse> GetWorkOrderByIdAsync(Guid workOrderId)
        {
            var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

            var workOrder = await workOrderRepo.FirstOrDefaultAsync(
                predicate: wo => wo.WorkOrderId == workOrderId,
                include: query => query
                    .Include(wo => wo.WarrantyClaim)
                        .ThenInclude(c => c.WarrantyPolicy)
                    .Include(wo => wo.WarrantyClaim)  // ✨ Thêm dòng này
                        .ThenInclude(c => c.CustomerVehicle)  // ✨ Quan trọng!
                    .Include(wo => wo.User)
                    .Include(wo => wo.Customer)
                    .Include(wo => wo.Parts)
                        .ThenInclude(p => p.PartItems)
            );

            if (workOrder == null)
                throw new Exception("Work order not found");

            var response = _mapper.Map<WorkOrderResponse>(workOrder);

            // Lấy tất cả PartItem liên quan đến các Parts trong WorkOrder
            if (workOrder.Parts != null && workOrder.Parts.Any())
            {
                response.PartItems = workOrder.Parts
                    .SelectMany(p => p.PartItems ?? new List<PartItem>())
                    .Select(pi => _mapper.Map<PartItemDto>(pi))
                    .ToList();
            }

            return response;
        }

        public async Task<List<WorkOrderResponse>> GetAllWorkOrdersAsync()
        {
            var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

            var workOrders = await workOrderRepo.GetListAsync(
                predicate: null, // Không filter
                orderBy: query => query.OrderByDescending(wo => wo.StartDate),
                include: query => query
                    .Include(wo => wo.WarrantyClaim)
                        .ThenInclude(c => c.CustomerVehicle)  
                    .Include(wo => wo.User)
                    .Include(wo => wo.Customer)
            );

            return _mapper.Map<List<WorkOrderResponse>>(workOrders);
        }

        public async Task<List<WorkOrderResponse>> GetWorkOrdersByPriorityAsync(WorkOrderPriority priority)
        {
            var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

            var workOrders = await workOrderRepo.GetListAsync(
                predicate: wo => wo.Priority == priority,
                orderBy: query => query.OrderByDescending(wo => wo.StartDate),
                include: query => query
                    .Include(wo => wo.WarrantyClaim)
                        .ThenInclude(c => c.WarrantyPolicy)
                    .Include(wo => wo.WarrantyClaim)  
                        .ThenInclude(c => c.CustomerVehicle)
                    .Include(wo => wo.User)
                    .Include(wo => wo.Customer)
                    .Include(wo => wo.Parts)
                        .ThenInclude(p => p.PartItems)
            );

            var responses = new List<WorkOrderResponse>();

            foreach (var workOrder in workOrders)
            {
                var response = _mapper.Map<WorkOrderResponse>(workOrder);

                if (workOrder.Parts != null && workOrder.Parts.Any())
                {
                    response.PartItems = workOrder.Parts
                        .SelectMany(p => p.PartItems ?? new List<PartItem>())
                        .Select(pi => _mapper.Map<PartItemDto>(pi))
                        .ToList();
                }

                responses.Add(response);
            }

            return responses;
        }

        #endregion

        #region Update WorkOrder

        public async Task<WorkOrderResponse> UpdateWorkOrderAsync(Guid workOrderId, UpdateWorkOrderRequest request)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

                var workOrder = await workOrderRepo.FirstOrDefaultAsync(
                    predicate: wo => wo.WorkOrderId == workOrderId,
                    include: query => query.Include(wo => wo.Parts)
                );

                if (workOrder == null)
                    throw new Exception("Work order not found");

                // Validate technician if changed
                if (request.TechnicianId.HasValue)
                {
                    var userRepo = _unitOfWork.GetRepository<User>();
                    var technician = await userRepo.FirstOrDefaultAsync(
                        predicate: u => u.UserId == request.TechnicianId.Value,
                        include: null
                    );

                    if (technician == null)
                        throw new Exception("Technician not found");
                }

                // Map changes to existing entity
                _mapper.Map(request, workOrder);

                // Update parts if provided
                if (request.PartIds != null)
                {
                    var partRepo = _unitOfWork.GetRepository<Part>();
                    var parts = await partRepo.GetListAsync(
                        predicate: p => request.PartIds.Contains(p.PartId),
                        include: null
                    );

                    workOrder.Parts = parts.ToList();
                }

                workOrderRepo.UpdateAsync(workOrder);
                await _unitOfWork.SaveChangesAsync();

                return await GetWorkOrderByIdAsync(workOrderId);
            });
        }

        public async Task<WorkOrderResponse> UpdateWorkOrderStatusAsync(Guid workOrderId, UpdateWorkOrderStatusRequest request)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

                var workOrder = await workOrderRepo.FirstOrDefaultAsync(
                    predicate: wo => wo.WorkOrderId == workOrderId,
                    include: null
                );

                if (workOrder == null)
                    throw new Exception("Work order not found");

                var oldStatus = workOrder.Status;
                var newStatus = request.Status;

                
                var validTransitions = new Dictionary<WorkOrderStatus, WorkOrderStatus[]>
                {
                    { WorkOrderStatus.Pending,    new[] { WorkOrderStatus.InProgress, WorkOrderStatus.Overdue } },
                    { WorkOrderStatus.InProgress, new[] { WorkOrderStatus.Completed, WorkOrderStatus.Overdue } },
                    { WorkOrderStatus.Completed,  Array.Empty<WorkOrderStatus>() },
                    { WorkOrderStatus.Overdue,    new[] { WorkOrderStatus.InProgress, WorkOrderStatus.Completed } }
                };

                if (!validTransitions.ContainsKey(oldStatus) ||
                    !validTransitions[oldStatus].Contains(newStatus))
                {
                    throw new Exception($"Invalid status transition from {oldStatus} to {newStatus}");
                }

                if (newStatus == WorkOrderStatus.Completed)
                {
                    workOrder.EndDate = DateTime.UtcNow;
                }

                
                workOrder.Status = newStatus;

                workOrderRepo.UpdateAsync(workOrder);
                await _unitOfWork.SaveChangesAsync();

                
                return await GetWorkOrderByIdAsync(workOrderId);
            });
        }


        #endregion

        #region Delete WorkOrder

        public async Task<bool> DeleteWorkOrderAsync(Guid workOrderId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

                var workOrder = await workOrderRepo.FirstOrDefaultAsync(
                    predicate: wo => wo.WorkOrderId == workOrderId,
                    include: null
                );

                if (workOrder == null)
                    throw new Exception("Work order not found");

                // Check if work order can be deleted (business rule)
                if (workOrder.Status == WorkOrderStatus.Completed)
                    throw new Exception("Cannot delete completed work order");

                workOrderRepo.DeleteAsync(workOrder);
                await _unitOfWork.SaveChangesAsync();

                return true;
            });
        }

        #endregion

        #region Assign Technician

        public async Task<WorkOrderResponse> AssignTechnicianAsync(Guid workOrderId, Guid technicianId)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Validate technician
                var userRepo = _unitOfWork.GetRepository<User>();
                var technician = await userRepo.FirstOrDefaultAsync(
                    predicate: u => u.UserId == technicianId,
                    include: null
                );

                if (technician == null)
                    throw new Exception("Technician not found");

                // Get work order
                var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();
                var workOrder = await workOrderRepo.FirstOrDefaultAsync(
                    predicate: wo => wo.WorkOrderId == workOrderId,
                    include: null
                );

                if (workOrder == null)
                    throw new Exception("Work order not found");

                // Assign technician
                workOrder.UserId = technicianId;

                // Update status to InProgress if still Pending
                if (workOrder.Status == WorkOrderStatus.Pending)
                    workOrder.Status = WorkOrderStatus.InProgress;

                workOrderRepo.UpdateAsync(workOrder);
                await _unitOfWork.SaveChangesAsync();

                return await GetWorkOrderByIdAsync(workOrderId);
            });
        }

        #endregion

        #region Get WorkOrders By Technician

        public async Task<List<WorkOrderResponse>> GetWorkOrdersByUserAsync(Guid userId)
        {
            var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

            var workOrders = await workOrderRepo.GetListAsync(
                predicate: wo => wo.UserId == userId,
                orderBy: query => query.OrderByDescending(wo => wo.StartDate),
                include: query => query
                    .Include(wo => wo.WarrantyClaim)
                        .ThenInclude(c => c.WarrantyPolicy)
                    .Include(wo => wo.WarrantyClaim)
                        .ThenInclude(c => c.CustomerVehicle)
                    .Include(wo => wo.User)
                    .Include(wo => wo.Customer)
                    .Include(wo => wo.Parts)
                        .ThenInclude(p => p.PartItems)
            );

            return _mapper.Map<List<WorkOrderResponse>>(workOrders);
        }


        public async Task<List<WorkOrderResponse>> GetWorkOrderByPriorityAsync(WorkOrderPriority priority)
        {
            var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

            var workOrders = await workOrderRepo.GetListAsync(
                predicate: wo => wo.Priority == priority,
                orderBy: query => query.OrderByDescending(wo => wo.StartDate),
                include: query => query
                    .Include(wo => wo.WarrantyClaim)
                        .ThenInclude(c => c.WarrantyPolicy)
                    .Include(wo => wo.WarrantyClaim)  
                        .ThenInclude(c => c.CustomerVehicle)
                    .Include(wo => wo.User)
                    .Include(wo => wo.Customer)
                    .Include(wo => wo.Parts)
                        .ThenInclude(p => p.PartItems)
            );

            var responses = new List<WorkOrderResponse>();

            foreach (var workOrder in workOrders)
            {
                var response = _mapper.Map<WorkOrderResponse>(workOrder);

                if (workOrder.Parts != null && workOrder.Parts.Any())
                {
                    response.PartItems = workOrder.Parts
                        .SelectMany(p => p.PartItems ?? new List<PartItem>())
                        .Select(pi => _mapper.Map<PartItemDto>(pi))
                        .ToList();
                }

                responses.Add(response);
            }

            return responses;
        }

        #endregion

        #region Get WorkOrders By Claim

        public async Task<WorkOrderResponse> GetWorkOrdersByClaimAsync(Guid claimId)
        {
            var workOrderRepo = _unitOfWork.GetRepository<WorkOrder>();

            var workOrders = await workOrderRepo.GetListAsync(
                predicate: wo => wo.ClaimId == claimId,
                orderBy: query => query.OrderByDescending(wo => wo.StartDate),
                include: query => query
                    .Include(wo => wo.WarrantyClaim)
                        .ThenInclude(c => c.WarrantyPolicy)
                    .Include(wo => wo.WarrantyClaim)
                        .ThenInclude(c => c.CustomerVehicle)
                    .Include(wo => wo.User)
                    .Include(wo => wo.Customer)
                    .Include(wo => wo.Parts)
                        .ThenInclude(p => p.PartItems)
            );

            return _mapper.Map<WorkOrderResponse>(workOrders);
        }

        #endregion
    }

    // Extension method for combining predicates
    public static class PredicateExtensions
    {
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            var combined = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );

            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }
    }
}
