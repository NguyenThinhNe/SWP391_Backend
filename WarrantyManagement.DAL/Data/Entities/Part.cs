using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public string Description { get; set; }

        [Required]
        public int Cost { get; set; }
        

        public Guid PolicyId { get; set; }

        public WorkOrder WorkOrder { get; set; }
    }
}
