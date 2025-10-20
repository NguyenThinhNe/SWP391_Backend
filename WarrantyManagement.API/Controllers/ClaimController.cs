using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IMapper _mapper;
        private readonly ILogger<ClaimController> _logger;
        public ClaimController(IClaimService petService, IMapper mapper, ILogger<ClaimController> logger)
        {
            _claimService = petService;
            _mapper = mapper;
            _logger = logger;
        }

        #region Get Claims
        /// <summary>
        /// Get claim by ID
        /// </summary>
        /// <param name="claimId">Claim ID</param>
        /// <returns>Claim details</returns>
        /// <response code="200">Returns the claim details</response>
        /// <response code="404">If claim is not found</response>
        [HttpGet("{claimId}")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> GetClaimById(Guid claimId)
        {
            try
            {
                var response = await _claimService.GetClaimByIdAsync(claimId);

                return Ok(new SuccessResponse<ClaimResponse>
                {
                    Success = true,
                    Message = "Claim retrieved successfully",
                    Data = response
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving the claim",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all claims with optional filters
        /// </summary>
        /// <param name="status">Filter by status</param>
        /// <param name="serviceCenterId">Filter by service center</param>
        /// <param name="vin">Filter by vehicle VIN</param>
        /// <param name="fromDate">Filter claims from date</param>
        /// <param name="toDate">Filter claims to date</param>
        /// <returns>List of claims</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<ClaimResponse>), 200)]
        public async Task<IActionResult> GetClaims(
            
            [FromQuery] Guid? serviceCenterId = null)
        {
            try
            {
                var response = await _claimService.GetClaimsAsync(
                 
                    serviceCenterId);

                return Ok(new SuccessResponse<ICollection<ClaimResponse>>
                {
                    Success = true,
                    Message = $"Retrieved {response.Count} claims",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving claims",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get claims by technician ID
        /// </summary>
        /// <param name="technicianId">Technician ID</param>
        /// <returns>List of claims created by the technician</returns>
        [HttpGet("technician/{technicianId}")]
        [ProducesResponseType(typeof(ICollection<ClaimResponse>), 200)]
        public async Task<IActionResult> GetClaimsByTechnician(Guid technicianId)
        {
            try
            {
                var response = await _claimService.GetClaimsByTechnicianAsync(technicianId);

                return Ok(new SuccessResponse<ICollection<ClaimResponse>>
                {
                    Success = true,
                    Message = $"Retrieved {response.Count} claims for technician",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving claims",
                    Details = ex.Message
                });
            }
        }
        /// <summary>
        /// Get claims by status
        /// </summary>
        /// <param name="Status">Status</param>
        /// <returns>List of claims searched by status</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetClaimsByStatus([FromQuery] WarrantyClaimStatus status)
        {
            try
            {
                var response = await _claimService.GetClaimsByStatusAsync(status);

                if (response == null || response.Count == 0)
                {
                    return Ok(new SuccessResponse<ICollection<ClaimResponse>>
                    {
                        Success = true,
                        Message = $"No claims found with status {status}",
                        Data = new List<ClaimResponse>()
                    });
                }

                return Ok(new SuccessResponse<ICollection<ClaimResponse>>
                {
                    Success = true,
                    Message = $"Retrieved {response.Count} claims with status {status}",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while retrieving claims",
                    Details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        #endregion

        #region Update Claim Status

        /// <summary>
        /// Start reviewing a claim - Changes status to InProgress
        /// </summary>
        /// <param name="claimId">Claim ID</param>
        /// <param name="evmStaffId">EVM Staff ID who is reviewing</param>
        /// <returns>Updated claim</returns>
        [HttpPut("{claimId}/start-review")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> StartReview(Guid claimId, [FromQuery] Guid evmStaffId)
        {
            try
            {
                if (evmStaffId == Guid.Empty)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "EVM Staff ID is required"
                    });
                }

                var response = await _claimService.StartReviewAsync(claimId, evmStaffId);

                return Ok(new SuccessResponse<ClaimResponse>
                {
                    Success = true,
                    Message = "Claim review started successfully",
                    Data = response
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while starting claim review",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Approve a claim - Changes status to Completed
        /// </summary>
        /// <param name="claimId">Claim ID</param>
        /// <param name="evmStaffId">EVM Staff ID who is approving</param>
        /// <returns>Updated claim</returns>
        [HttpPut("{claimId}/approve")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> ApproveClaim(Guid claimId, [FromQuery] Guid evmStaffId)
        {
            try
            {
                if (evmStaffId == Guid.Empty)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "EVM Staff ID is required"
                    });
                }

                var response = await _claimService.ApproveClaimAsync(claimId, evmStaffId);

                return Ok(new SuccessResponse<ClaimResponse>
                {
                    Success = true,
                    Message = "Claim approved successfully",
                    Data = response
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while approving claim",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Reject a claim
        /// </summary>
        /// <param name="claimId">Claim ID</param>
        /// <param name="request">Rejection request with EVM Staff ID and reason</param>
        /// <returns>Updated claim</returns>
        [HttpPut("{claimId}/reject")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> RejectClaim(
            Guid claimId,
            [FromBody] RejectClaimRequestWithStaff request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var response = await _claimService.RejectClaimAsync(
                    claimId,
                    request.EvmStaffId,
                    request.RejectionReason);

                return Ok(new SuccessResponse<ClaimResponse>
                {
                    Success = true,
                    Message = "Claim rejected successfully",
                    Data = response
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while rejecting claim",
                    Details = ex.Message
                });
            }
        }

        /// <summary>
        /// Update claim status manually
        /// </summary>
        /// <param name="claimId">Claim ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Updated claim</returns>
        [HttpPut("{claimId}/status")]
        [ProducesResponseType(typeof(ClaimResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> UpdateClaimStatus(
            Guid claimId,
            [FromBody] UpdateClaimStatusRequestWithUser request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var response = await _claimService.UpdateClaimStatusAsync(
                    claimId,
                    request.NewStatus,
                    request.UserId);

                return Ok(new SuccessResponse<ClaimResponse>
                {
                    Success = true,
                    Message = "Claim status updated successfully",
                    Data = response
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while updating claim status",
                    Details = ex.Message
                });
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Check if vehicle is eligible for warranty claim
        /// </summary>
        /// <param name="vin">Vehicle VIN</param>
        /// <param name="policyId">Policy ID</param>
        /// <returns>Eligibility status</returns>
        [HttpGet("validate-eligibility")]
        [ProducesResponseType(typeof(WarrantyEligibilityResponse), 200)]
        public async Task<IActionResult> ValidateWarrantyEligibility(
            [FromQuery] string vin,
            [FromQuery] Guid policyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vin))
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "VIN is required"
                    });
                }

                if (policyId == Guid.Empty)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Policy ID is required"
                    });
                }

                var isEligible = await _claimService.ValidateWarrantyEligibilityAsync(vin, policyId);

                return Ok(new SuccessResponse<WarrantyEligibilityResponse>
                {
                    Success = true,
                    Message = isEligible
                        ? "Vehicle is eligible for warranty claim"
                        : "Vehicle is not eligible for warranty claim",
                    Data = new WarrantyEligibilityResponse
                    {
                        VIN = vin,
                        PolicyId = policyId,
                        IsEligible = isEligible
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = "An error occurred while validating warranty eligibility",
                    Details = ex.Message
                });
            }
        }

        #endregion
    }
}
