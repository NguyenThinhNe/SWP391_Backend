using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.BLL.Services.Interfaces;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/vehicle")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        /// <summary>
        /// Lấy toàn bộ danh sách xe (kèm thông tin khách hàng)
        /// </summary>
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _vehicleService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin xe theo VIN
        /// </summary>
        [HttpGet("get-by-vin/{vin}")]
        public async Task<IActionResult> GetByVin(string vin)
        {
            var result = await _vehicleService.GetByVinAsync(vin);
            if (result == null)
                return NotFound($"Không tìm thấy xe có VIN = {vin}");

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách xe theo tên khách hàng (họ hoặc tên)
        /// </summary>
        [HttpGet("get-by-customer-name")]
        public async Task<IActionResult> GetByCustomerName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Tên khách hàng không được để trống.");

            var result = await _vehicleService.GetByCustomerNameAsync(name);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách xe theo ID khách hàng
        /// </summary>
        [HttpGet("get-by-customer-id/{customerId}")]
        public async Task<IActionResult> GetByCustomerId(Guid customerId)
        {
            var result = await _vehicleService.GetByCustomerIdAsync(customerId);
            return Ok(result);
        }
    }
}
