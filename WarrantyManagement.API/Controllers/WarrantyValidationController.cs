using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/policy")]
    [ApiController]
    public class WarrantyValidationController : ControllerBase
    {
        private readonly IWarrantyValidationService _validationService;
        private readonly ILogger<WarrantyValidationController> _logger;
        public WarrantyValidationController(IWarrantyValidationService validationService, ILogger<WarrantyValidationController> logger)
        {
            _validationService = validationService;
            _logger = logger;
        }


        /// <summary>
        /// Kiểm tra policy có còn hoạt động cho part item cụ thể không
        /// </summary>
        /// <param name="policyId">ID của warranty policy</param>
        /// <param name="partItemId">ID của part item</param>
        /// <returns>Kết quả validation</returns>
        [HttpGet("validate")]
        [ProducesResponseType(typeof(SuccessResponse<WarrantyValidationDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> ValidateWarranty(
            [FromQuery] Guid policyId,
            [FromQuery] Guid partItemId)
        {
            try
            {
                // Validation input
                if (policyId == Guid.Empty)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        Details = "Policy ID is required",
                        Errors = new List<string> { "PolicyId cannot be empty" }
                    });
                }

                if (partItemId == Guid.Empty)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        Details = "Part Item ID is required",
                        Errors = new List<string> { "PartItemId cannot be empty" }
                    });
                }

                var result = await _validationService.ValidatePolicyForPartItemAsync(policyId, partItemId);

                return Ok(new SuccessResponse<WarrantyValidationDto>
                {
                    Success = result.IsValid,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating warranty for PolicyId: {PolicyId}, PartItemId: {PartItemId}",
                    policyId, partItemId);

                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while validating warranty",
                    Details = ex.Message,
                    Errors = new List<string> { ex.InnerException?.Message ?? ex.Message }
                });
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái bảo hành của part item (tự động tìm policy)
        /// </summary>
        /// <param name="partItemId">ID của part item</param>
        /// <returns>Kết quả validation</returns>
        [HttpGet("check-status/{partItemId}")]
        [ProducesResponseType(typeof(SuccessResponse<WarrantyValidationDto>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> CheckWarrantyStatus(Guid partItemId)
        {
            try
            {
                if (partItemId == Guid.Empty)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        Details = "Part Item ID is required",
                        Errors = new List<string> { "PartItemId cannot be empty" }
                    });
                }

                var result = await _validationService.CheckWarrantyStatusAsync(partItemId);

                // Nếu không tìm thấy part item
                if (result.PartItem == null)
                {
                    return NotFound(new ErrorResponse
                    {
                        Message = "Resource not found",
                        Details = result.Message,
                        Errors = new List<string> { $"Part item with ID {partItemId} not found" }
                    });
                }

                return Ok(new SuccessResponse<WarrantyValidationDto>
                {
                    Success = result.IsValid,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking warranty status for PartItemId: {PartItemId}", partItemId);

                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while checking warranty status",
                    Details = ex.Message,
                    Errors = new List<string> { ex.InnerException?.Message ?? ex.Message }
                });
            }
        }

        /// <summary>
        /// Kiểm tra trạng thái bảo hành của nhiều part items cùng lúc
        /// </summary>
        /// <param name="partItemIds">Danh sách ID của part items</param>
        /// <returns>Danh sách kết quả validation</returns>
        [HttpPost("check-multiple")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<WarrantyValidationDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> CheckMultipleWarrantyStatus([FromBody] List<Guid> partItemIds)
        {
            try
            {
                // Validation
                if (partItemIds == null || !partItemIds.Any())
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        Details = "Part item IDs list cannot be empty",
                        Errors = new List<string> { "At least one part item ID is required" }
                    });
                }

                // Kiểm tra có Guid.Empty không
                var emptyGuids = partItemIds.Where(id => id == Guid.Empty).ToList();
                if (emptyGuids.Any())
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        Details = "Invalid part item IDs detected",
                        Errors = new List<string> { $"Found {emptyGuids.Count} empty GUID(s) in the request" }
                    });
                }

                // Giới hạn số lượng items có thể check cùng lúc
                if (partItemIds.Count > 100)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        Details = "Too many items requested",
                        Errors = new List<string> { "Maximum 100 part items can be checked at once" }
                    });
                }

                var results = await _validationService.CheckMultiplePartItemsAsync(partItemIds);
                var resultsList = results.ToList();

                return Ok(new SuccessResponse<IEnumerable<WarrantyValidationDto>>
                {
                    Success = true,
                    Message = $"Successfully validated {resultsList.Count} part item(s). " +
                             $"Valid: {resultsList.Count(r => r.IsValid)}, Invalid: {resultsList.Count(r => !r.IsValid)}",
                    Data = resultsList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking multiple warranty statuses");

                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while checking multiple warranty statuses",
                    Details = ex.Message,
                    Errors = new List<string> { ex.InnerException?.Message ?? ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách part items có warranty sắp hết hạn
        /// </summary>
        /// <param name="daysThreshold">Số ngày còn lại để coi là "sắp hết hạn" (mặc định: 30 ngày)</param>
        /// <returns>Danh sách part items có warranty sắp hết hạn</returns>
        [HttpGet("expiring-soon")]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<WarrantyValidationDto>>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetExpiringWarranties([FromQuery] int daysThreshold = 30)
        {
            try
            {
                if (daysThreshold <= 0)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        Details = "Days threshold must be greater than 0",
                        Errors = new List<string> { "daysThreshold must be a positive number" }
                    });
                }

                if (daysThreshold > 365)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Validation failed",
                        Details = "Days threshold is too large",
                        Errors = new List<string> { "daysThreshold cannot exceed 365 days" }
                    });
                }

                var results = await _validationService.GetExpiringWarrantiesAsync(daysThreshold);
                var resultsList = results.ToList();

                return Ok(new SuccessResponse<IEnumerable<WarrantyValidationDto>>
                {
                    Success = true,
                    Message = resultsList.Any()
                        ? $"Found {resultsList.Count} part item(s) with warranty expiring within {daysThreshold} days"
                        : $"No part items with warranty expiring within {daysThreshold} days",
                    Data = resultsList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring warranties with threshold: {DaysThreshold}", daysThreshold);

                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving expiring warranties",
                    Details = ex.Message,
                    Errors = new List<string> { ex.InnerException?.Message ?? ex.Message }
                });
            }
        }
    }
}
