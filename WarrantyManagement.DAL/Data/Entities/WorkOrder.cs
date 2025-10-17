using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class WorkOrder
    {
        [Key]
        public Guid WorkId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public int EstimateHour { get; set; }

        [Required]
        public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Pending;

        [Required]
        public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Low;

        [Required]
        public Guid ClaimId { get; set; }
        [ForeignKey(nameof(ClaimId))]
        public WarrantyClaim WarrantyClaim { get; set; }

        public Guid PartId { get; set; }
        [ForeignKey(nameof(PartId))]
        public ICollection<Part> Parts { get; set; }

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        
        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }
    }
}
