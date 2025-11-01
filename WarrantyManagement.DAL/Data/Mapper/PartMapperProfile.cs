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
    public class PartMapperProfile : Profile
    {
        public PartMapperProfile()
        {
            CreateMap<VehiclePart, PartResponseDto>()
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.Part.PartId))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part.PartName))
                .ForMember(dest => dest.PartDescription, opt => opt.MapFrom(src => src.Part.Description))
                .ForMember(dest => dest.VehiclePartId, opt => opt.MapFrom(src => src.VehiclePartId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.VehiclePartDescription, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.Vehicle.VIN))
                .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src => src.Vehicle.VehicleName))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Vehicle.Model));

        }
    }
}
