using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.DAL.Data.Mapper
{
    public class CampaignMappingProfile : Profile
    {
        public CampaignMappingProfile() {
            CreateMap<CreateCampainRequest, Campaign>()
                .ForMember(dest => dest.CampaignId, opt => opt.Ignore()) //set in service
                .ForMember(dest => dest.CampaignName, opt => opt.MapFrom(src => src.CampaignName))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CustomerVehicles, opt => opt.Ignore());
             CreateMap<Campaign, CampaignResponse>()
                .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.CampaignId))
                .ForMember(dest => dest.CampaignName, opt => opt.MapFrom(src => src.CampaignName))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.TechnicanId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ServiceCenterId, opt => opt.MapFrom(src => src.User != null ? src.User.ServiceCenterId : (Guid?)null))
     
                .ForMember(dest => dest.Vehicles,
                     opt => opt.MapFrom(src => src.CustomerVehicles ?? new List<CustomerVehicle>()));

            CreateMap<CustomerVehicle, VehicleBasicInfo>()
                .ForMember(dest => dest.Vin, opt => opt.MapFrom(src => src.VIN));

            CreateMap<UpdateCampaignRequest, Campaign>()
                .ForMember(dest => dest.CampaignId, opt => opt.Ignore()) //set in service
                .ForMember(dest => dest.CampaignName, opt => opt.MapFrom(src => src.CampaignName))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CustomerVehicles, opt => opt.Ignore());
        }
    }
}
