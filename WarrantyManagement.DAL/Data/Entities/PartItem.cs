using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class PartItem
    {
        [Key]
        public Guid PartItemId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PartNumber { get; set; }
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;
        
        [Required]
        public DateTime EndDate { get; set; }

        public Guid PartId { get; set; }
        [ForeignKey(nameof(PartId))]
        public Part Part { get; set; }

        public Guid ClaimId { get; set; }
        [ForeignKey(nameof(ClaimId))]
        public WarrantyClaim WarrantyClaim { get; set; }
        public Guid InventoryId { get; set; }
        [ForeignKey(nameof(InventoryId))]
        public Inventory Inventory { get; set; }
    }
}
