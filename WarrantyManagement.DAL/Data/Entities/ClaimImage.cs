using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class ClaimImage
    {
        [Key]
        public Guid ImageId { get; set; }

        [Required]
        public Guid ClaimId { get; set; }
        [ForeignKey(nameof(ClaimId))]
        public WarrantyClaim WarrantyClaim { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public string? Description { get; set; }
        public int OrderIndex { get; set; }
    }
}
