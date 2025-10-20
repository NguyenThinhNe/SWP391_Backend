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
                .ForMember(dest => dest.ClaimId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => WarrantyClaimStatus.Pending))
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // vẫn ignore để set trong service
                .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => src.PolicyId))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.IssueDescription, opt => opt.MapFrom(src => src.IssueDescription))
                .ForMember(dest => dest.ClaimDescription, opt => opt.MapFrom(src => src.ClaimDescription))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<WarrantyClaim, ClaimResponse>()
                // Direct mappings
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

                // Part information from PartItems (get first part item)
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src =>
                    src.PartItems != null && src.PartItems.Any()
                        ? src.PartItems.First().PartId
                        : Guid.Empty))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src =>
                    src.PartItems != null && src.PartItems.Any() && src.PartItems.First().Part != null
                        ? src.PartItems.First().Part.PartName
                        : string.Empty))
                .ForMember(dest => dest.PartNumber, opt => opt.MapFrom(src =>
                    src.PartItems != null && src.PartItems.Any()
                        ? src.PartItems.First().PartNumber
                        : string.Empty))

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
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
        }

    }
}

