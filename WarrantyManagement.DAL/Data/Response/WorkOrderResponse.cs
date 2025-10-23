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

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } // Từ Customer
        public string CustomerPhone { get; set; }

        // Parts information
        public List<PartSummaryDto> Parts { get; set; }
        public List<PartItemDto> PartItems { get; set; }
        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
