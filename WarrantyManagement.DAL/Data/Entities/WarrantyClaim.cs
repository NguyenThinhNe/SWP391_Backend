using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class WarrantyClaim
    {
        [Key]
        [Required]
        public Guid ClaimId { get; set; }
        
        [Required]
        public DateTime ClaimDate { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string ClaimDescription { get; set; }

        [Required]
        [MaxLength(200)]
        public string IssueDescription { get; set; }
        
        [Required]
        [ForeignKey(nameof(VIN))]
        public string VIN {  get; set; }
        public CustomerVehicle CustomerVehicle { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        
        [Required]
        public Guid PolicyId { get; set; }
        [ForeignKey(nameof(PolicyId))]
        public WarrantyPolicy WarrantyPolicy { get; set; }

        public ICollection<PartItem> PartItems { get; set; }

        public enum ClaimStatus
        {
            Pending,
            InProgress,
            Completed,
            Overdue
        }
    }
}
