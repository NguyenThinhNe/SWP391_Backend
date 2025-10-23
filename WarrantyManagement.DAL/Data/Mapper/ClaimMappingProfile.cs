using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Enums;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.DAL.Data.Mapper
{
    public class ClaimMappingProfile : Profile
    {
        public ClaimMappingProfile()
        {
            CreateMap<ClaimRequest, WarrantyClaim>()
                 .ForMember(dest => dest.ClaimId, opt => opt.Ignore()) // Set in service
                 .ForMember(dest => dest.ClaimDate, opt => opt.MapFrom(src => src.ClaimDate))
                 .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                 .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => src.PolicyId))
                 .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set in service
                 .ForMember(dest => dest.User, opt => opt.Ignore()) // Navigation property
                 .ForMember(dest => dest.CustomerVehicle, opt => opt.Ignore()) // Navigation property
                 .ForMember(dest => dest.WarrantyPolicy, opt => opt.Ignore()) // Navigation property
                 .ForMember(dest => dest.PartItems, opt => opt.Ignore()); // Handled in service

            // ✅ WarrantyClaim → ClaimResponse
            CreateMap<WarrantyClaim, ClaimResponse>()
                // Direct claim fields
                .ForMember(dest => dest.ClaimId, opt => opt.MapFrom(src => src.ClaimId))
                .ForMember(dest => dest.ClaimDate, opt => opt.MapFrom(src => src.ClaimDate))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.ClaimStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IssueDescription, opt => opt.MapFrom(src => src.IssueDescription))
                .ForMember(dest => dest.ClaimDescription, opt => opt.MapFrom(src => src.ClaimDescription))

                // Vehicle information from CustomerVehicle
                .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src =>
                    src.CustomerVehicle != null ? src.CustomerVehicle.VehicleName : string.Empty))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src =>
                    src.CustomerVehicle != null ? src.CustomerVehicle.PurchaseDate : DateTime.MinValue))
                .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src =>
                    src.CustomerVehicle != null ? src.CustomerVehicle.MileAge : 0))

                // Parts information (multiple parts)
                .ForMember(dest => dest.Parts, opt => opt.MapFrom(src => src.PartItems))

                // Policy information
                .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => src.PolicyId))
                .ForMember(dest => dest.PolicyName, opt => opt.MapFrom(src =>
                    src.WarrantyPolicy != null ? src.WarrantyPolicy.Name : string.Empty))

                // Service Center information 
                .ForMember(dest => dest.ServiceCenterId, opt => opt.MapFrom(src =>
                    src.User != null && src.User.ServiceCenterId.HasValue
                        ? src.User.ServiceCenterId.Value
                        : Guid.Empty))
                .ForMember(dest => dest.ServiceCenterName, opt => opt.MapFrom(src =>
                    src.User != null && src.User.ServiceCenter != null
                        ? src.User.ServiceCenter.CenterName
                        : string.Empty))

                // User information (Technician)
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.TechnicianName, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Name : string.Empty));

            // ✅ PartItem → PartItemResponse
            CreateMap<PartItem, PartItemResponse>()
                .ForMember(dest => dest.PartItemId, opt => opt.MapFrom(src => src.PartItemId))
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src =>
                    src.Part != null ? src.Part.PartName : string.Empty))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                    src.Part != null ? src.Part.Description : string.Empty))
                .ForMember(dest => dest.Cost, opt => opt.MapFrom(src =>
                    src.Part != null ? src.Part.Cost : 0))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src =>
                    src.Part != null ? src.Part.Cost * src.Quantity : 0));
                
        }

    }
}

