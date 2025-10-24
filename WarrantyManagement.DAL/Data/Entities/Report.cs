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
    public class Report
    {
        [Key]
        public Guid ReportId { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        [Required]
        public double TotalPrice { get; set; } = 0;
        
        [Required]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        [Required]
        public Guid ClaimId { get; set; }
        [ForeignKey(nameof(ClaimId))]
        public WarrantyClaim WarrantyClaim { get; set; }

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

    }
}
