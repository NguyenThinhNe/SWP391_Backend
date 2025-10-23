using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Request
{
    public class WorkOrderRequest
    {
        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        public Guid TechnicianId { get; set; } // UserId của SC Technician được assign

        [Required]
        public Guid CustomerId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int EstimateHour { get; set; }

        public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Low;

        // Optional: Danh sách part IDs nếu biết trước
        public List<Guid> PartIds { get; set; }
    }
}
