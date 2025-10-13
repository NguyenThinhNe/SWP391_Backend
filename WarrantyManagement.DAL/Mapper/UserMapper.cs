using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.DAL.Mapper
{
    public class UserMapper
    {
        private readonly IMapper _mapper;

        public UserMapper(IMapper mapper)
        {
            _mapper = mapper;
        }

        // For Map create user request
        public User MapToEntity(CreateUserRequest request)
        {
            return _mapper.Map<User>(request);
        }

        public void MapToExistingEntity(UpdateUserRequest request, User user)
        {
            // Có thể map từng field hoặc để AutoMapper handle:
            _mapper.Map(request, user);
        }

        public UserResponse MapToResponse(User user)
        {
            return _mapper.Map<UserResponse>(user);
        }

        public List<UserResponse> MapToResponseList(List<User> users)
        {
            return _mapper.Map<List<UserResponse>>(users);
        }
    }
}
