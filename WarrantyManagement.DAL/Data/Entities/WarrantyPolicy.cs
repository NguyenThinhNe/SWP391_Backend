using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class WarrantyPolicy
    {
        [Key]
        [Required]
        public Guid PolicyId { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(300)]
        public string Description { get; set; }

        [Required]
        public int DurationMonth { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Condition { get; set; }
        
        [Required]
        public DateTime CreateTime { get; set; } = DateTime.Now;
        
        public ICollection<WarrantyClaim> warrantyClaims { get; set; }
    }
}
