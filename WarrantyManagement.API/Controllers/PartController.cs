using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.BLL.Services.Interfaces;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/part")]
    [ApiController]
    public class PartController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartController(IPartService partService)
        {
            _partService = partService;
        }

        // GET: api/part
        [HttpGet]
        [Authorize(Roles = "SCTech,SCStaff,EVMStaff")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _partService.GetAllPartsAsync();
            return Ok(result);
        }

        // GET: api/part/{vin}
        [HttpGet("{vin}")]
        [Authorize(Roles = "SCTech,SCStaff,EVMStaff")]
        public async Task<IActionResult> GetByVIN(string vin)
        {
            var result = await _partService.GetPartsByVINAsync(vin);
            if (result == null || !result.Any())
                return NotFound(new { message = $"No parts found for vehicle with VIN: {vin}" });

            return Ok(result);
        }

        // GET: api/part/detail/{partId}
        [HttpGet("{partId:guid}")]
        [Authorize(Roles = "SCTech,SCStaff,EVMStaff")]
        public async Task<IActionResult> GetByPartId(Guid partId)
        {
            var result = await _partService.GetPartByIdAsync(partId);
            if (result == null)
                return NotFound(new { message = $"Part not found with ID: {partId}" });

            return Ok(result);
        }
    }
}
