using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Response;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.BLL.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork<WarrantyDbContext> _unitOfWork;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork<WarrantyDbContext> unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            // lấy repository cho User (UnitOfWork sẽ tạo/đưa lại nếu đã có)
            _userRepository = _unitOfWork.GetRepository<User>();
        }

        public async Task<IEnumerable<UserResponse>> GetUsersByServiceCenterAsync(Guid centerId)
        {
            // include ServiceCenter để lấy CenterName
            Func<IQueryable<User>, IIncludableQueryable<User, object>> include = q => q.Include(u => u.ServiceCenter);

            var users = await _userRepository.GetListAsync(
                predicate: u => u.ServiceCenterId == centerId,
                include: include
            );

            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }

        public async Task<IEnumerable<UserResponse>> GetUsersByRoleAsync(UserRole role)
        {
            // include ServiceCenter để map ServiceCenterName
            Func<IQueryable<User>, IIncludableQueryable<User, object>> include = q => q.Include(u => u.ServiceCenter);

            var users = await _userRepository.GetListAsync(
                predicate: u => u.Role == role,
                include: include
            );

            return _mapper.Map<IEnumerable<UserResponse>>(users);
        }
        public async Task<IEnumerable<UserResponse>> GetTechniciansAsync()
        {
            var include = new Func<IQueryable<User>, IIncludableQueryable<User, object>>(q => q.Include(u => u.ServiceCenter));

            var technicians = await _userRepository.GetListAsync(
                predicate: u => u.Role == UserRole.SCTech,
                include: include
            );

            return _mapper.Map<IEnumerable<UserResponse>>(technicians);
        }
    }
}
