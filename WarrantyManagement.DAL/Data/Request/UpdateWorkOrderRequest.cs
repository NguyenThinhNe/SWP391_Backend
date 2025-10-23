using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Request
{
    public class UpdateWorkOrderRequest
    {
        [MaxLength(200)]
        public string Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? EstimateHour { get; set; }

        public WorkOrderStatus? Status { get; set; }

        public WorkOrderPriority? Priority { get; set; }

        public Guid? TechnicianId { get; set; } // Reassign technician

        public List<Guid> PartIds { get; set; } // Cập nhật parts
    }


    

}
