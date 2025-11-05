using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/users")]
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
        [Authorize(Roles= "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersByRole(UserRole role)
        {
            var users = await _userService.GetUsersByRoleAsync(role);

            if (users == null || !users.Any())
                return NotFound(new { message = "Không tìm thấy người dùng nào với role này." });

            return Ok(users);
        }
        [HttpGet("technicians")]
        [Authorize(Roles = "SCStaff,Admin")]
        public async Task<IActionResult> GetTechnicians()
        {
            var users = await _userService.GetTechniciansAsync();

            if (users == null || !users.Any())
                return NotFound(new { message = "Không tìm thấy kỹ thuật viên nào." });

            return Ok(users);
        }
        // <summary>
        /// Activate or deactivate a user account.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="isActive">true to activate, false to deactivate</param>
        /// <returns>Updated user info</returns>
        [HttpPut("{userId}/active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetUserActiveStatus(Guid userId, [FromQuery] bool isActive)
        {
            try
            {
                var updatedUser = await _userService.ToggleUserActiveStatusAsync(userId, isActive);

                return Ok(new
                {
                    message = isActive
                        ? "User account activated successfully."
                        : "User account deactivated successfully.",
                    data = updatedUser
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user status.", error = ex.Message });
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users.", error = ex.Message });
            }
        }

        // GET: api/users/active
        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetActiveUsers()
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active users.", error = ex.Message });
            }
        }
    }
}
