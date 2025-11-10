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
    public class PolicyMappingProfile : Profile
    {
        public PolicyMappingProfile()
        {
            CreateMap<WarrantyPolicy, WarrantyPolicyDto>();
         

        }
    }
}
