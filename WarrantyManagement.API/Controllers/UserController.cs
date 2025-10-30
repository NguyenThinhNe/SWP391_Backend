using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Lấy tất cả người dùng thuộc 1 Service Center (theo ID)
        /// </summary>
        /// <param name="centerId">ID trung tâm dịch vụ</param>
        /// <returns>Danh sách người dùng</returns>
        [HttpGet("by-center/{centerId}")]
        public async Task<IActionResult> GetUsersByServiceCenter(Guid centerId)
        {
            var users = await _userService.GetUsersByServiceCenterAsync(centerId);

            if (users == null || !users.Any())
                return NotFound(new { message = "Không tìm thấy người dùng nào thuộc trung tâm này." });

            return Ok(users);
        }

        /// <summary>
        /// Lấy tất cả người dùng theo vai trò (Admin, SCTech, SCStaff, EVMStaff)
        /// </summary>
        /// <param name="role">Tên role</param>
        /// <returns>Danh sách người dùng</returns>
        [HttpGet("by-role/{role}")]
        public async Task<IActionResult> GetUsersByRole(UserRole role)
        {
            var users = await _userService.GetUsersByRoleAsync(role);

            if (users == null || !users.Any())
                return NotFound(new { message = "Không tìm thấy người dùng nào với role này." });

            return Ok(users);
        }
        [HttpGet("technicians")]
        public async Task<IActionResult> GetTechnicians()
        {
            var users = await _userService.GetTechniciansAsync();

            if (users == null || !users.Any())
                return NotFound(new { message = "Không tìm thấy kỹ thuật viên nào." });

            return Ok(users);
        }
    }
}
