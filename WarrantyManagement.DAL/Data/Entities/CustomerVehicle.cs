using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class CustomerVehicle
    {
        [Key]
        public string VIN {  get; set; }

        [Required]
        public string VehicleName { get; set; }

        [Required]
        public string Model { get; set; }

        public DateTime PurchaseDate { get; set; }
        
        public int MileAge { get; set; }

        [Required]
        public Guid CampaignId { get; set; }
        [ForeignKey(nameof(CampaignId))]
        public Campaign Campaign { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        public ICollection<VehiclePart> VehicleParts { get; set; }
    }
}
