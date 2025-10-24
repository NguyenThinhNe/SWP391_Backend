using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class Inventory
    {
        [Key]
        public Guid InventoryId { get; set; }

        [Required, MaxLength(100)]
        public string PartName { get; set; }

        
        public ICollection<PartItem> PartItems { get; set; }

        [Required]
        public Guid CenterId { get; set; }
        [ForeignKey(nameof(CenterId))]
        public ServiceCenter ServiceCenter { get; set; }
    }
}
