using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class VehiclePart
    {
        [Key]
        public Guid VehiclePartId { get; set; }

        [Required]
        public int Quantity { get; set; } = 0;
        
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        public bool Status { get; set; }

        [Required]
        public string VIN {  get; set; }
        [ForeignKey(nameof(VIN))]
        public CustomerVehicle Vehicle { get; set; }
    }
}
