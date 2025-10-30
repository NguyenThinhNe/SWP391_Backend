using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Response
{
    public class WorkOrderResponse
    {
        public Guid WorkOrderId { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int EstimateHour { get; set; }
        public WorkOrderStatus Status { get; set; }
        public string StatusDisplay { get; set; } // "Pending", "In Progress", "Completed"...
        public WorkOrderPriority Priority { get; set; }
        public string PriorityDisplay { get; set; } // "Low", "Medium", "High"...

        // Related entities info
        public Guid ClaimId { get; set; }

        public Guid TechnicianId { get; set; }
        public string TechnicianName { get; set; } // Từ User
        //Customer information
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } // Từ Customer
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }

        // Vehicle information
        public string VIN { get; set; }
        public string VehicleName { get; set; }
        public string Model { get; set; }
        public int MileAge { get; set; }
        public DateTime PurchaseDate { get; set; }

        //Service center information
        public Guid ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; }

        // Parts information
        public List<PartSummaryDto> Parts { get; set; }
        public List<PartItemDto> PartItems { get; set; }
        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
