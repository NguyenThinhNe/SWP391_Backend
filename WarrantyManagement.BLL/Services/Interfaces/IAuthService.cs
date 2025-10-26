using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<SuccessResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<SuccessResponse<AuthUserResponse>> GetCurrentUserAsync(Guid userId);
        Task<SuccessResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        Task<SuccessResponse<bool>> ValidateTokenAsync(string token);
    }
}
