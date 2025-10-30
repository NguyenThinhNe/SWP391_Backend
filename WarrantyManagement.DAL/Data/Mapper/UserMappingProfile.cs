using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.DAL.Data.Mapper
{
    public class UserMappingProfile: Profile
    {
        public UserMappingProfile() 
        {
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.ServiceCenterName,
                    opt => opt.MapFrom(src => src.ServiceCenter != null ? src.ServiceCenter.CenterName : null));
        }

    }
}
