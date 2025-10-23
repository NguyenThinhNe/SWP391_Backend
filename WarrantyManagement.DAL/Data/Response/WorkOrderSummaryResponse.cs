using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Response
{
    public class WorkOrderSummaryResponse
    {
        public Guid WorkOrderId { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public WorkOrderStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public WorkOrderPriority Priority { get; set; }
        public string PriorityDisplay { get; set; }
        public string ClaimNumber { get; set; }
        public string TechnicianName { get; set; }
        public string CustomerName { get; set; }
    }
}
