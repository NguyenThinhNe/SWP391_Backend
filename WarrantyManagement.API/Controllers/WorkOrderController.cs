using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/work-orders")]
    [ApiController]
    public class WorkOrderController : ControllerBase
    {
        private readonly IWorkOrderService _workOrderService;

        public WorkOrderController(IWorkOrderService workOrderService)
        {
            _workOrderService = workOrderService;
        }

        #region Create WorkOrder

        /// <summary>
        /// Create a new work order (SC Staff assigns to Technician)
        /// </summary>
        /// <param name="request">Work order creation details</param>
        /// <returns>Created work order</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SuccessResponse<WorkOrderResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateWorkOrder([FromBody] WorkOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid request data",
                        Details = "Please check your input fields",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _workOrderService.CreateWorkOrderAsync(request);

                return CreatedAtAction(
                    nameof(GetWorkOrderById),
                    new { id = result.WorkOrderId },
                    new SuccessResponse<WorkOrderResponse>
                    {
                        Success = true,
                        Message = "Work order created successfully",
                        Data = result
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Failed to create work order",
                    Details = ex.Message,
                    Errors = new List<string> { ex.ToString() }
                });
            }
        }

        #endregion

        #region Get WorkOrders

        /// <summary>
        /// Get work order by ID
        /// </summary>
        /// <param name="id">Work order ID</param>
        /// <returns>Work order details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<WorkOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkOrderById(Guid id)
        {
            try
            {
                var result = await _workOrderService.GetWorkOrderByIdAsync(id);

                return Ok(new SuccessResponse<WorkOrderResponse>
                {
                    Success = true,
                    Message = "Work order retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Work order not found",
                    Details = ex.Message,
                    Errors = new List<string> { $"No work order found with ID: {id}" }
                });
            }
        }

        /// <summary>
        /// Get paginated list of work orders with filters
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <returns>Paginated work orders</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SuccessResponse<PagedWorkOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkOrders([FromQuery] GetWorkOrdersRequest request)
        {
            try
            {
                var result = await _workOrderService.GetWorkOrdersAsync(request);

                return Ok(new SuccessResponse<PagedWorkOrderResponse>
                {
                    Success = true,
                    Message = "Work orders retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Failed to retrieve work orders",
                    Details = ex.Message,
                    Errors = new List<string> { ex.ToString() }
                });
            }
        }

        /// <summary>
        /// Get work orders assigned to a specific technician
        /// </summary>
        /// <param name="technicianId">Technician user ID</param>
        /// <returns>List of work orders</returns>
        [HttpGet("technician/{technicianId}")]
        [ProducesResponseType(typeof(SuccessResponse<List<WorkOrderSummaryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkOrdersByTechnician(Guid technicianId)
        {
            try
            {
                var result = await _workOrderService.GetWorkOrdersByTechnicianAsync(technicianId);

                return Ok(new SuccessResponse<List<WorkOrderSummaryResponse>>
                {
                    Success = true,
                    Message = "Technician work orders retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Failed to retrieve technician work orders",
                    Details = ex.Message,
                    Errors = new List<string> { ex.ToString() }
                });
            }
        }

        /// <summary>
        /// Get work orders for a specific claim
        /// </summary>
        /// <param name="claimId">Claim ID</param>
        /// <returns>List of work orders</returns>
        [HttpGet("claim/{claimId}")]
        [ProducesResponseType(typeof(SuccessResponse<List<WorkOrderSummaryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkOrdersByClaim(Guid claimId)
        {
            try
            {
                var result = await _workOrderService.GetWorkOrdersByClaimAsync(claimId);

                return Ok(new SuccessResponse<List<WorkOrderSummaryResponse>>
                {
                    Success = true,
                    Message = "Claim work orders retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "Failed to retrieve claim work orders",
                    Details = ex.Message,
                    Errors = new List<string> { ex.ToString() }
                });
            }
        }


        /// <summary>
        /// Get work orders by priority
        /// GET: api/workorder/by-priority/{priority}
        /// </summary>
        /// <param name="priority">0=Low, 1=Medium, 2=High</param>
        [HttpGet("by-priority/{priority}")]
        public async Task<ActionResult<List<WorkOrderResponse>>> GetWorkOrderByPriority(WorkOrderPriority priority)
        {
            try
            {
                var workOrders = await _workOrderService.GetWorkOrderByPriorityAsync(priority);

                return Ok(new
                {
                    success = true,
                    message = $"Lấy danh sách work orders với độ ưu tiên '{priority}' thành công",
                    data = workOrders,
                    total = workOrders.Count,
                    priority = priority.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách work orders",
                    error = ex.Message
                });
            }
        }

        #endregion

        #region Update WorkOrder

        /// <summary>
        /// Update work order details (SC Staff/Admin)
        /// </summary>
        /// <param name="id">Work order ID</param>
        /// <param name="request">Update details</param>
        /// <returns>Updated work order</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<WorkOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWorkOrder(Guid id, [FromBody] UpdateWorkOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid request data",
                        Details = "Please check your input fields",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _workOrderService.UpdateWorkOrderAsync(id, request);

                return Ok(new SuccessResponse<WorkOrderResponse>
                {
                    Success = true,
                    Message = "Work order updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Failed to update work order",
                    Details = ex.Message,
                    Errors = new List<string> { $"Work order with ID {id} not found or update failed" }
                });
            }
        }

        /// <summary>
        /// Update work order status (Technician)
        /// </summary>
        /// <param name="id">Work order ID</param>
        /// <param name="request">Status update details</param>
        /// <returns>Updated work order</returns>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(SuccessResponse<WorkOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateWorkOrderStatus(Guid id, [FromBody] UpdateWorkOrderStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid request data",
                        Details = "Please check your input fields",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _workOrderService.UpdateWorkOrderStatusAsync(id, request);

                return Ok(new SuccessResponse<WorkOrderResponse>
                {
                    Success = true,
                    Message = "Work order status updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Failed to update work order status",
                    Details = ex.Message,
                    Errors = new List<string> { $"Work order with ID {id} not found" }
                });
            }
        }

        /// <summary>
        /// Assign or reassign technician to work order (SC Staff)
        /// </summary>
        /// <param name="id">Work order ID</param>
        /// <param name="request">Assignment details</param>
        /// <returns>Updated work order</returns>
        [HttpPatch("{id}/assign")]
        [ProducesResponseType(typeof(SuccessResponse<WorkOrderResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignTechnician(Guid id, [FromBody] AssignTechnicianRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid request data",
                        Details = "Please check your input fields",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _workOrderService.AssignTechnicianAsync(id, request.TechnicianId);

                return Ok(new SuccessResponse<WorkOrderResponse>
                {
                    Success = true,
                    Message = "Technician assigned successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Failed to assign technician",
                    Details = ex.Message,
                    Errors = new List<string> { $"Work order or technician not found" }
                });
            }
        }

        #endregion

        #region Delete WorkOrder

        /// <summary>
        /// Delete work order
        /// </summary>
        /// <param name="id">Work order ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(SuccessResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWorkOrder(Guid id)
        {
            try
            {
                var result = await _workOrderService.DeleteWorkOrderAsync(id);

                return Ok(new SuccessResponse<bool>
                {
                    Success = true,
                    Message = "Work order deleted successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Failed to delete work order",
                    Details = ex.Message,
                    Errors = new List<string> { $"Work order with ID {id} not found or cannot be deleted" }
                });
            }
        }

        #endregion

        #region
        public class AssignTechnicianRequest
        {
            [Required(ErrorMessage = "Technician ID is required")]
            public Guid TechnicianId { get; set; }
        }
        #endregion
    }
}
