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
                .ForMember(dest => dest.ClaimId, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.ClaimDate, opt => opt.MapFrom(src => src.ClaimDate))
                .ForMember(dest => dest.ClaimDescription, opt => opt.MapFrom(src => src.ClaimDescription))
                .ForMember(dest => dest.IssueDescription, opt => opt.MapFrom(src => src.IssueDescription))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => WarrantyClaimStatus.Pending))
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Set in service layer
                .ForMember(dest => dest.PolicyId, opt => opt.Ignore()) // Set in service layer
                .ForMember(dest => dest.CustomerVehicle, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.WarrantyPolicy, opt => opt.Ignore())
                .ForMember(dest => dest.PartItems, opt => opt.Ignore());

            // Map WarrantyClaim entity to ClaimResponse
            CreateMap<WarrantyClaim, ClaimResponse>()
                .ForMember(dest => dest.ClaimId, opt => opt.MapFrom(src => src.ClaimId))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.ClaimStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src =>
                    src.CustomerVehicle != null ? src.CustomerVehicle.VehicleName : string.Empty));
        }

    }
}

