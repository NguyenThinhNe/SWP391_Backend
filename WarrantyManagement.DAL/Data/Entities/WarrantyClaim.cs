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
    public class WarrantyClaim
    {
        [Key]
        [Required]
        public Guid ClaimId { get; set; }
        
        [Required]
        public DateTime ClaimDate { get; set; } = DateTime.Now;

        [Required]
        public WarrantyClaimStatus Status { get; set; } = WarrantyClaimStatus.Pending;

        [Required]
        [MaxLength(400)]
        public string IssueDescription { get; set; }
        public bool isActive { get; set; } = true;
        [Required]
        public string VIN {  get; set; }
        [ForeignKey(nameof(VIN))]
        public CustomerVehicle CustomerVehicle { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
        
        [Required]
        public Guid PolicyId { get; set; }
        [ForeignKey(nameof(PolicyId))]
        public WarrantyPolicy WarrantyPolicy { get; set; }

        public ICollection<ClaimDetail> ClaimDetails { get; set; }
    }
}
