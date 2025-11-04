using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class Campaign
    {
        [Key]
        public Guid CampaignId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string CampaignName { get; set; }
        
        [MaxLength(200)]
        public string Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public CampaignStatus Status { get; set; } = CampaignStatus.Pending;

        public ICollection<CustomerVehicle> CustomerVehicles { get; set; }


    }
}
