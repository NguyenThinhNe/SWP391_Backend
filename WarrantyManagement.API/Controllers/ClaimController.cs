using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/claims")]
    [ApiController]
    public class ClaimController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        /// <summary>
        /// Create a new warranty claim
        /// </summary>
        /// <param name="request">Claim request with selected parts</param>
        /// <returns>Created claim response</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Authorize(Roles = "SCTech")]
        public async Task<IActionResult> CreateClaim(
            [FromBody] ClaimRequest request,
            [FromQuery] Guid technicianId) // TODO: Get from JWT token after authentication
        {
            try
            {
                // Validate technicianId
                if (technicianId == Guid.Empty)
                {
                    return BadRequest(new { message = "Technician ID is required" });
                }

                // Create the claim
                var result = await _claimService.CreateClaimAsync(request, technicianId);

                return Ok(new
                {
                    success = true,
                    message = "Claim created successfully",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in CreateClaim: {ex}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while creating the claim",
                    error = ex.Message,
                    detail = ex.InnerException?.Message
                });
            }
        }
        /// <summary>
        /// Update warranty claim (Only SCTech can update claims with Pending status)
        /// </summary>
        /// <param name="claimId">Claim ID to update</param>
        /// <param name="request">Update claim request</param>
        /// <returns>Updated claim information</returns>
        [HttpPut("{claimId}")]
        [Authorize(Roles = "SCTech")]
        [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClaim(
            [FromRoute] Guid claimId,
            [FromBody] UpdateClaimRequest request)
        {
            try
            {
                // Validate ModelState
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                // Validate ClaimId
                if (claimId == Guid.Empty)
                {
                    return BadRequest(new { message = "ClaimId không hợp lệ" });
                }

                // Get current user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                // Call service to update claim
                var result = await _claimService.UpdateClaimAsync(request, claimId);

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật claim thành công",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                // Business logic errors (claim not found, wrong status, validation errors)
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi cập nhật claim",
                    error = ex.Message
                });
            }
        }
        /// <summary>
        /// Get claim by ID
        /// </summary>
        [HttpGet("{claimId}")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "SCTech,SCStaff,EVMStaff")]
        public async Task<IActionResult> GetClaimById(Guid claimId)
        {
            try
            {
                var result = await _claimService.GetClaimByIdAsync(claimId);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all claims with optional service center filter
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<ClaimResponse>), 200)]
        [Authorize(Roles = "SCTech,SCStaff,EVMStaff")]
        public async Task<IActionResult> GetClaims()
        {
            try
            {
                var results = await _claimService.GetClaimsAsync();
                return Ok(new
                {
                    success = true,
                    count = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while retrieving claims",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get claims by service center
        /// </summary>
        [HttpGet("service-center/{serviceCenterId}")]
        [ProducesResponseType(typeof(ICollection<ClaimResponse>), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff")]
        public async Task<IActionResult> GetClaimsByServiceCenter(Guid serviceCenterId)
        {
            try
            {
                var results = await _claimService.GetClaimsAsync(serviceCenterId);
                return Ok(new
                {
                    success = true,
                    count = results.Count,
                    data = results
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get claims by user
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ICollection<ClaimResponse>), 200)]
        [Authorize(Roles = "SCTech,EVMStaff,SCStaff")]
        public async Task<IActionResult> GetClaimsByUser(Guid userId)
        {
            try
            {
                var results = await _claimService.GetClaimsByUserAsync(userId);
                return Ok(new
                {
                    success = true,
                    count = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get claims by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ICollection<ClaimResponse>), 200)]
        [Authorize(Roles = "SCTech,SCStaff,EVMStaff")]
        public async Task<IActionResult> GetClaimsByStatus(WarrantyClaimStatus status)
        {
            try
            {
                var results = await _claimService.GetClaimsByStatusAsync(status);
                return Ok(new
                {
                    success = true,
                    count = results.Count,
                    data = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        /// <summary>
        /// Approve a claim
        /// </summary>
        [HttpPut("{claimId}/approve")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff")]
        public async Task<IActionResult> ApproveClaim(Guid claimId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user authentication" });
                }

                var result = await _claimService.ApproveClaimAsync(claimId, userId);
                return Ok(new
                {
                    success = true,
                    message = "Claim approved successfully",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Reject a claim
        /// </summary>
        [HttpPut("{claimId}/reject")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff")]
        public async Task<IActionResult> RejectClaim(
            Guid claimId,
            [FromBody] RejectClaimRequestWithStaff  request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user authentication" });
                }

                var result = await _claimService.RejectClaimAsync(
                    claimId,
                    userId,
                    request.RejectionReason);

                return Ok(new
                {
                    success = true,
                    message = "Claim rejected",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Update claim status
        /// </summary>
        [HttpPut("{claimId}/status")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff")]
        public async Task<IActionResult> UpdateClaimStatus(
            Guid claimId,
            [FromBody] UpdateClaimStatusRequestWithUser request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new { message = "Invalid or missing user authentication" });
                }

                var result = await _claimService.UpdateClaimStatusAsync(
                    claimId,
                    request.NewStatus,
                    userId);

                return Ok(new
                {
                    success = true,
                    message = "Claim status updated",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a warranty claim by ID
        /// </summary>
        /// <param name="claimId">The claim ID to delete</param>
        /// <returns>Boolean indicating success or failure</returns>
        [HttpDelete("{claimId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "SCTech, EVMStaff")]
        public async Task<IActionResult> DeleteClaim(Guid claimId)
        {
            try
            {
                var result = await _claimService.DeleteClaimAsync(claimId);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = $"Claim with ID {claimId} not found"
                    });
                }

                return Ok(new
                {
                    message = "Claim deleted successfully",
                    claimId = claimId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred while deleting the claim",
                    error = ex.Message
                });
            }
        }
    }
}
