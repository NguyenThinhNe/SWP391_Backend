using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.DAL.Data.Mapper
{
    public class PartItemMappingProfile: Profile
    {
        public PartItemMappingProfile()
        {
            // Mapping từ CreatePartItemDto sang PartItem entity
            CreateMap<CreatePartItemDto, PartItem>()
                .ForMember(dest => dest.PartItemId, opt => opt.Ignore())
                .ForMember(dest => dest.Part, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore())
                .ForMember(dest => dest.ClaimDetails, opt => opt.Ignore());

            // Mapping từ UpdatePartItemDto sang PartItem entity
            CreateMap<UpdatePartItemDto, PartItem>()
                .ForMember(dest => dest.Part, opt => opt.Ignore())
                .ForMember(dest => dest.Inventory, opt => opt.Ignore())
                .ForMember(dest => dest.ClaimDetails, opt => opt.Ignore());

            // Mapping từ PartItem entity sang PartItemDto
            CreateMap<PartItem, PartItemDto>()
                 .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part != null ? src.Part.PartName : null))
                 .ForMember(dest => dest.ClaimId, opt => opt.MapFrom(src =>
                     src.ClaimDetails != null && src.ClaimDetails.Any()
                         ? src.ClaimDetails.First().ClaimId
                         : Guid.Empty))
                 .ForMember(dest => dest.InventoryId, opt => opt.MapFrom(src =>
                     src.InventoryId.HasValue ? src.InventoryId.Value : Guid.Empty));

            // Mapping từ PartItem entity sang PartItemDetailDto
            CreateMap<PartItem, PartItemDetailDto>()
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.Part != null ? src.Part.PartName : null))
                .ForMember(dest => dest.PartDescription, opt => opt.MapFrom(src => src.Part != null ? src.Part.Description : null))
                .ForMember(dest => dest.InventoryName, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.PartName : null))
                .ForMember(dest => dest.ServiceCenterId, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.CenterId : (Guid?)null))
                .ForMember(dest => dest.ServiceCenterName, opt => opt.MapFrom(src => src.Inventory != null && src.Inventory.ServiceCenter != null ? src.Inventory.ServiceCenter.CenterName : null))
                .ForMember(dest => dest.ClaimDetailCount, opt => opt.MapFrom(src => src.ClaimDetails != null ? src.ClaimDetails.Count : 0));
        }
    }
}
