using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;

namespace WarrantyManagement.DAL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUserNameAsync(string username);
        // Get User by Email
        Task<User> GetByEmailAsync(string email);   
        Task<List<User>> GetByRoleAsync(UserRole role);
        Task<List<User>> GetByServiceCenterIdAsync(Guid serviceCenterId);
        Task<bool> ExistsByUserNameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);

    }
}
