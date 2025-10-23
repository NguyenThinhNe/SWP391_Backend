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
    public class OrderMappingProfile: Profile
    {
        public OrderMappingProfile()
        {
            // ==================== REQUEST TO ENTITY ====================

            // CreateWorkOrderRequest -> WorkOrder
            CreateMap<WorkOrderRequest, WorkOrder>()
                .ForMember(dest => dest.WorkOrderId, opt => opt.Ignore()) // Auto-generated
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.TechnicianId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => WorkOrderStatus.Pending))
                .ForMember(dest => dest.WarrantyClaim, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Parts, opt => opt.Ignore()); // Handle separately in service

            // UpdateWorkOrderRequest -> WorkOrder (for updating existing entity)
            CreateMap<UpdateWorkOrderRequest, WorkOrder>()
                .ForMember(dest => dest.WorkOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.ClaimId, opt => opt.Ignore()) // Cannot change claim
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Cannot change customer
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.TechnicianId ?? Guid.Empty))
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
                .ForMember(dest => dest.StartDate, opt => opt.Condition(src => src.StartDate.HasValue))
                .ForMember(dest => dest.EndDate, opt => opt.Condition(src => src.EndDate.HasValue))
                .ForMember(dest => dest.EstimateHour, opt => opt.Condition(src => src.EstimateHour.HasValue))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status.HasValue))
                .ForMember(dest => dest.Priority, opt => opt.Condition(src => src.Priority.HasValue))
                .ForMember(dest => dest.WarrantyClaim, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Parts, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // UpdateWorkOrderStatusRequest -> WorkOrder (partial update for status only)
            CreateMap<UpdateWorkOrderStatusRequest, WorkOrder>()
                .ForMember(dest => dest.WorkOrderId, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.EndDate, opt => opt.Ignore())
                .ForMember(dest => dest.EstimateHour, opt => opt.Ignore())
                .ForMember(dest => dest.Priority, opt => opt.Ignore())
                .ForMember(dest => dest.ClaimId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.WarrantyClaim, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Parts, opt => opt.Ignore());

            // ==================== ENTITY TO RESPONSE ====================

            // WorkOrder -> WorkOrderResponse (detailed response)
            CreateMap<WorkOrder, WorkOrderResponse>()
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PriorityDisplay, opt => opt.MapFrom(src => src.Priority.ToString()))
                
                .ForMember(dest => dest.TechnicianId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.TechnicianName, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Name : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer != null ? src.Customer.LastName : string.Empty))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src =>
                    src.Customer != null ? src.Customer.PhoneNumber : string.Empty))
                .ForMember(dest => dest.Parts, opt => opt.MapFrom(src =>
                    src.Parts != null ? src.Parts.ToList() : new System.Collections.Generic.List<Part>()))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Add if you have audit fields

            // WorkOrder -> WorkOrderSummaryResponse (list view)
            CreateMap<WorkOrder, WorkOrderSummaryResponse>()
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PriorityDisplay, opt => opt.MapFrom(src => src.Priority.ToString()))
                
                .ForMember(dest => dest.TechnicianName, opt => opt.MapFrom(src =>
                    src.User != null ? src.User.Name : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer != null ? src.Customer.LastName : string.Empty));

            // ==================== PART MAPPINGS ====================

            // Part -> PartSummaryDto
            CreateMap<Part, PartSummaryDto>()
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src => src.PartName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost))
                .ForMember(dest => dest.PolicyId, opt => opt.MapFrom(src => src.PolicyId))
                // Aggregate data from PartItems
                .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src =>
                    src.PartItems != null ? src.PartItems.Sum(pi => pi.Quantity) : 0))
                .ForMember(dest => dest.PartNumbers, opt => opt.MapFrom(src =>
                    src.PartItems != null ? src.PartItems.Select(pi => pi.PartNumber).ToList() : new System.Collections.Generic.List<string>()))
                .ForMember(dest => dest.PartItemCount, opt => opt.MapFrom(src =>
                    src.PartItems != null ? src.PartItems.Count : 0));

            // PartItem -> PartItemDto (for detailed part item information)
            CreateMap<PartItem, PartItemDto>()
                .ForMember(dest => dest.PartItemId, opt => opt.MapFrom(src => src.PartItemId))
                .ForMember(dest => dest.PartNumber, opt => opt.MapFrom(src => src.PartNumber))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.PartId, opt => opt.MapFrom(src => src.PartId))
                .ForMember(dest => dest.PartName, opt => opt.MapFrom(src =>
                    src.Part != null ? src.Part.PartName : string.Empty))
                .ForMember(dest => dest.ClaimId, opt => opt.MapFrom(src => src.ClaimId));
                
        }
    }
}
