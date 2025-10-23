using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IWorkOrderService
    {
        Task<WorkOrderResponse> CreateWorkOrderAsync(WorkOrderRequest request);

        // Read
        Task<WorkOrderResponse> GetWorkOrderByIdAsync(Guid workOrderId);
        Task<PagedWorkOrderResponse> GetWorkOrdersAsync(GetWorkOrdersRequest request);
        Task<List<WorkOrderSummaryResponse>> GetWorkOrdersByTechnicianAsync(Guid technicianId);
        Task<List<WorkOrderSummaryResponse>> GetWorkOrdersByClaimAsync(Guid claimId);

        // Update
        Task<WorkOrderResponse> UpdateWorkOrderAsync(Guid workOrderId, UpdateWorkOrderRequest request);
        Task<WorkOrderResponse> UpdateWorkOrderStatusAsync(Guid workOrderId, UpdateWorkOrderStatusRequest request);
        Task<WorkOrderResponse> AssignTechnicianAsync(Guid workOrderId, Guid technicianId);

        // Delete
        Task<bool> DeleteWorkOrderAsync(Guid workOrderId);
    }
}
