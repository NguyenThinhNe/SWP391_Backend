using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Request;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/[controller]")]
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

        [HttpPost]
        public async Task<IActionResult> CreateClaim([FromBody] ClaimRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                
                var userId = Guid.NewGuid();

                var result = await _claimService.CreateClaimAsync(request, userId);
                _logger.LogInformation("Tạo yêu cầu bảo hành thành công cho xe VIN: {VIN}", request.VIN);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo yêu cầu bảo hành.");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAllClaims()
        {
            try
            {
                var result = await _claimService.GetAllClaimsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách yêu cầu bảo hành.");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi truy xuất dữ liệu." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClaimById(Guid id)
        {
            try
            {
                var result = await _claimService.GetClaimByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Không tìm thấy yêu cầu bảo hành với ID: {ClaimId}", id);
                    return NotFound(new { message = "Không tìm thấy yêu cầu bảo hành." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết yêu cầu bảo hành với ID: {ClaimId}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi truy xuất dữ liệu." });
            }
        }
    }
}
