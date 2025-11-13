using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/campaigns")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }
        /// <summary>
        /// Create a new campaign
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CampaignResponse), 201)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "EVMStaff")]
        public async Task<IActionResult> CreateCampaign([FromBody] CreateCampainRequest request)
        {
            try
            {
                var result = await _campaignService.CreateCampaignAsync(request);
                return CreatedAtAction(nameof(GetCampaignById), new { id = result.CampaignId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the campaign", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing campaign
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CampaignResponse), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff")]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateCampaign(Guid id, [FromBody] UpdateCampaignRequest request)
        {
            try
            {
                var result = await _campaignService.UpdateCampaignAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the campaign", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a campaign
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "EVMStaff")]
        public async Task<IActionResult> DeleteCampaign(Guid id)
        {
            try
            {
                var result = await _campaignService.DeleteCampaignAsync(id);
                if (!result)
                    return NotFound(new { message = $"Campaign with ID {id} not found" });

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the campaign", error = ex.Message });
            }
        }

        /// <summary>
        /// Get campaign by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CampaignResponse), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff,SCTech,SCStaff")]
        public async Task<IActionResult> GetCampaignById(Guid id)
        {
            try
            {
                var result = await _campaignService.GetCampaignByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the campaign", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all campaigns
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CampaignResponse>), 200)]
        [Authorize(Roles = "EVMStaff,SCTech,SCStaff")]
        public async Task<IActionResult> GetAllCampaigns()
        {
            try
            {
                var result = await _campaignService.GetAllCampaignsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving campaigns", error = ex.Message });
            }
        }

        /// <summary>
        /// Get first campaign by status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(CampaignResponse), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff,SCTech,SCStaff")]
        public async Task<IActionResult> GetCampaignByStatus(CampaignStatus status)
        {
            try
            {
                var result = await _campaignService.GetCampaignByStatusAsync(status);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving campaign", error = ex.Message });
            }
        }

        /// <summary>
        /// Get active campaigns
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<CampaignResponse>), 200)]
        [Authorize(Roles = "EVMStaff,SCTech,SCStaff")]
        public async Task<IActionResult> GetActiveCampaigns()
        {
            try
            {
                var result = await _campaignService.GetActiveCampaignsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active campaigns", error = ex.Message });
            }
        }

        /// <summary>
        /// Update campaign status
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "SCTech")]
        public async Task<IActionResult> UpdateCampaignStatus(Guid id, [FromBody] CampaignStatus newStatus)
        {
            try
            {
                var result = await _campaignService.UpdateCampaignStatusAsync(id, newStatus);
                if (!result)
                    return NotFound(new { message = $"Campaign with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating campaign status", error = ex.Message });
            }
        }

        /// <summary>
        /// Add vehicles to campaign
        /// </summary>
        [HttpPost("{id}/vehicles")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "SCStaff")]
        public async Task<IActionResult> AddVehiclesToCampaign(Guid id, [FromBody] List<string> vehicleVins)
        {
            try
            {
                await _campaignService.AddVehiclesToCampaignAsync(id, vehicleVins);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding vehicles to campaign", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove vehicles from campaign
        /// </summary>
        [HttpDelete("{id}/vehicles")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "SCStaff")]
        public async Task<IActionResult> RemoveVehiclesFromCampaign(Guid id, [FromBody] List<string> vehicleVins)
        {
            try
            {
                var result = await _campaignService.RemoveVehiclesFromCampaignAsync(id, vehicleVins);
                if (!result)
                    return NotFound(new { message = $"No vehicles found to remove from campaign {id}" });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing vehicles from campaign", error = ex.Message });
            }
        }
        /// <summary>
        /// Assign a technician to a campaign
        /// </summary>
        [HttpPost("{id}/technicians/{technicianId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "SCStaff")]
        public async Task<IActionResult> AssignTechnician(Guid id, Guid technicianId)
        {
            var updated = await _campaignService.AssignTechnicianAsync(id, technicianId);
            return Ok(updated);
        }
        /// <summary>
        /// Get all campaigns assigned to a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<CampaignResponse>), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff,SCTech,SCStaff")]
        public async Task<IActionResult> GetCampaignsByUserId(Guid userId)
        {
            try
            {
                var result = await _campaignService.GetCampaignByUserId(userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving campaigns by user", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all campaigns under a specific service center
        /// </summary>
        [HttpGet("service-center/{serviceCenterId}")]
        [ProducesResponseType(typeof(IEnumerable<CampaignResponse>), 200)]
        [ProducesResponseType(404)]
        [Authorize(Roles = "EVMStaff,SCStaff,SCTech")]
        public async Task<IActionResult> GetCampaignsByServiceCenterId(Guid serviceCenterId)
        {
            try
            {
                var result = await _campaignService.GetCampaignByServiceCenterId(serviceCenterId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving campaigns by service center", error = ex.Message });
            }
        }
    }
}
