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
                .ForMember(dest => dest.ClaimId, opt => opt.Ignore()) // set in service
                .ForMember(dest => dest.ClaimDate, opt => opt.MapFrom(src => src.ClaimDate))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.IssueDescription, opt => opt.MapFrom(src => src.IssueDescription))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => WarrantyClaimStatus.Pending))
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // set in service
                .ForMember(dest => dest.PolicyId, opt => opt.Ignore()) // set in service (from vehicle/policy)
                .ForMember(dest => dest.Images, opt => opt.Ignore())                                                      // Don't try to map nested CustomerVehicle object here; handle in service
                .ForMember(dest => dest.CustomerVehicle, opt => opt.Ignore())
                .ForMember(dest => dest.ClaimDetails, opt => opt.Ignore())
                .ForAllOtherMembers(opt => opt.Ignore()); // be explicit - only map above


            // ✅ PartItemRequest → PartItem (for creating new PartItem)
            CreateMap<PartItemRequest, PartItem>()
                .ForMember(dest => dest.PartItemId, opt => opt.Ignore()) // Auto-generated
                .ForPath(dest => dest.Part.PartName, opt => opt.MapFrom(src => src.PartName))
                .ForMember(dest => dest.PartNumber, opt => opt.MapFrom(src => src.PartNumber))

                .ForMember(dest => dest.Part, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Inventory, opt => opt.Ignore()) // Set in service if needed
                .ForMember(dest => dest.InventoryId, opt => opt.Ignore()) // Set in service if needed
                .ForMember(dest => dest.ClaimDetails, opt => opt.Ignore()); // Handled in service
                                                                            // Update claim request to entity mapping can be added here if needed
            CreateMap<UpdateClaimRequest, WarrantyClaim>()
               .ForMember(dest => dest.ClaimDate, opt => opt.MapFrom(src => src.ClaimDate))
               .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
               .ForMember(dest => dest.IssueDescription, opt => opt.MapFrom(src => src.IssueDescription))
               
               .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => src.PolicyId))
               .ForMember(dest => dest.ClaimDetails, opt => opt.Ignore()) // handled separately
               .ForMember(dest => dest.Status, opt => opt.Ignore())       // don’t override current status
               .ForMember(dest => dest.UserId, opt => opt.Ignore())       // assigned from logged user
               .ForMember(dest => dest.CustomerVehicle, opt => opt.Ignore())
               .ForMember(dest => dest.WarrantyPolicy, opt => opt.Ignore());
            // ✅ WarrantyClaim → ClaimResponse
            CreateMap<WarrantyClaim, ClaimResponse>()
                // Direct claim fields
                .ForMember(dest => dest.ClaimId, opt => opt.MapFrom(src => src.ClaimId))
                .ForMember(dest => dest.ClaimDate, opt => opt.MapFrom(src => src.ClaimDate))
                .ForMember(dest => dest.isActive, opt => opt.MapFrom(src => src.isActive))
                .ForMember(dest => dest.VIN, opt => opt.MapFrom(src => src.VIN))
                .ForMember(dest => dest.ClaimStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IssueDescription, opt => opt.MapFrom(src => src.IssueDescription))
                // Vehicle information from CustomerVehicle
                .ForMember(dest => dest.VehicleName, opt => opt.MapFrom(src =>
                    src.CustomerVehicle != null ? src.CustomerVehicle.VehicleName : string.Empty))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src =>
                    src.CustomerVehicle != null ? src.CustomerVehicle.PurchaseDate : DateTime.MinValue))
                .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src =>
                    src.CustomerVehicle != null ? src.CustomerVehicle.MileAge : 0))

                // Parts information (through ClaimDetails -> PartItem)
                .ForMember(dest => dest.Parts, opt => opt.MapFrom(src =>
                    src.ClaimDetails != null
                        ? src.ClaimDetails.Select(cd => cd.PartItem).ToList()
                        : new List<PartItem>()))

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
                    src.User != null ? src.User.Name : string.Empty))

                // TotalCost is calculated in ClaimResponse itself
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ✅ PartItem → PartItemResponse
            CreateMap<PartItem, PartItemResponse>()
                .ForMember(dest => dest.PartItemId, opt => opt.MapFrom(src => src.PartItemId))
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId))
                .ForMember(dest => dest.PartNumber, opt => opt.MapFrom(src => src.PartNumber))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src =>
                    src.Part != null ? src.Part.PartName : string.Empty));

            CreateMap<ClaimImage, CLaimImageResponse>()
                .ForMember(dest => dest.ImageId, opt => opt.MapFrom(src => src.ImageId))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.OrderIndex, opt => opt.MapFrom(src => src.OrderIndex));
        }

    }
}

