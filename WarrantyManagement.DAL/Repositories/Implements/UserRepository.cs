using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.DAL.Repositories.Implements
{
    
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(WarrantyDbContext context) : base(context)
        {
        }

        public async Task<User> GetByUserNameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetByRoleAsync(UserRole role)
        {
            return await _dbSet.Where(u => (UserRole)u.Role == role).ToListAsync();
        }

        public async Task<List<User>> GetByServiceCenterIdAsync(Guid serviceCenterId)
        {
            return await _dbSet
                .Where(u => u.ServiceCenterId == serviceCenterId)
                .ToListAsync();
        }

        public async Task<bool> ExistsByUserNameAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.UserName == username);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }
    }
}

