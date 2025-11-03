using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponse>> GetUsersByServiceCenterAsync(Guid centerId);
        Task<IEnumerable<UserResponse>> GetUsersByRoleAsync(UserRole role);
        Task<IEnumerable<UserResponse>> GetTechniciansAsync();
        Task<UserResponse> ToggleUserActiveStatusAsync(Guid userId, bool isActive);
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<IEnumerable<UserResponse>> GetActiveUsersAsync();
    }
}
