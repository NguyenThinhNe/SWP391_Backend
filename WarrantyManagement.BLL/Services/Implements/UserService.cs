using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;
using WarrantyManagement.DAL.Mapper;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.BLL.Services.Implements
{
    public class UserService : IUserService
    {
        //private readonly IUnitOfWork _unitOfWork;
        //private readonly UserMapper _userMapper;

        //public UserService(IUnitOfWork unitOfWork, UserMapper userMapper)
        //{
        //    _unitOfWork = unitOfWork;
        //    _userMapper = userMapper;
        //}

        //public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        //{
        //    if (await _unitOfWork.Users.ExistsByUserNameAsync(request.UserName))
        //        throw new Exception("Username already exists.");

        //    var user = _userMapper.MapToEntity(request);
        //    user.UserId = Guid.NewGuid();
        //    user.CreatedTime = DateTime.UtcNow;
        //    user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

        //    await _unitOfWork.Users.AddAsync(user);
        //    await _unitOfWork.SaveChangesAsync();

        //    return _userMapper.MapToResponse(user);
        //}

        //public Task<bool> DeleteUserAsync(Guid userId)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<List<UserResponse>> GetUsersByRoleAsync(UserRole role)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<List<UserResponse>> GetUsersByServiceCenterAsync(Guid serviceCenterId)
        //{
        //    throw new NotImplementedException();
        //}

        //public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
        //{
        //    var user = await _unitOfWork.Users.GetByIdAsync(id);
        //    if (user == null) throw new Exception("User not found.");

        //    _userMapper.MapToExistingEntity(request, user);
        //    await _unitOfWork.SaveChangesAsync();

        //    return _userMapper.MapToResponse(user);
        //}
    }
}
