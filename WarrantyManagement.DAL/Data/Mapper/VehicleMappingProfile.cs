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
    public class VehicleMappingProfile : Profile
    {
        public VehicleMappingProfile()
        {
            CreateMap<CustomerVehicle, VehicleResponse>()
               // Map các trường từ entity CustomerVehicle
               .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
               .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src => src.VehicleName))
               .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
               .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.PurchaseDate))
               .ForMember(dest => dest.MileAge, opt => opt.MapFrom(src => src.MileAge))
               // Map các trường từ entity Customer
               .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Customer.CustomerId))
               .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Customer.FirstName))
               .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Customer.LastName))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Customer.PhoneNumber))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Customer.Email))
               // Ignore các quan hệ không cần thiết (Campaign, VehicleParts)
               .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
