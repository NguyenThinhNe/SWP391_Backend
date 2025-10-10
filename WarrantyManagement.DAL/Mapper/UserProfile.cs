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
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<CreateUserRequest, User>();
            CreateMap<UpdateUserRequest, User>();
            CreateMap<User, UserResponse>();
        }
    }
}
