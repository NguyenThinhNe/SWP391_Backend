using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class Part
    {
        [Key]
        public Guid PartId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartName { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
        
        public Guid PolicyId { get; set; }
        [ForeignKey(nameof(PolicyId))]
        public WarrantyPolicy WarrantyPolicy { get; set; }

        public Guid WorkOrderId { get; set; }
        [ForeignKey(nameof(WorkOrderId))]
        public WorkOrder WorkOrder { get; set; }

        public ICollection<PartItem> PartItems { get; set; }
        public ICollection<VehiclePart> VehicleParts { get; set; }
    }
}
