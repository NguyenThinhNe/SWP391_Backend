using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Guid ClaimId { get; set; }
        [ForeignKey(nameof(ClaimId))]
        public WarrantyClaim WarrantyClaim { get; set; }

        public Guid PartId { get; set; }
        [ForeignKey(nameof(PartId))]
        public ICollection<Part> Parts { get; set; }
        
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        
        public Guid CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        public enum Priority
        {
            Low, Medium, High
        }

        public enum Status
        {
            Pending,
            InProgress,
            Completed,
            Overdue
        }
    }
}
