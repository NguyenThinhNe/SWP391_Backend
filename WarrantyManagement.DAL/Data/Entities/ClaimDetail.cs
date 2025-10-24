using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class ClaimDetail

    {
        [Key]
        public Guid ClaimDetailId { get; set; }

        [Required]
        public Guid ClaimId { get; set; }
        [ForeignKey(nameof(ClaimId))]
        public WarrantyClaim WarrantyClaim { get; set; }

        [Required]
        public Guid PartItemId { get; set; }
        [ForeignKey(nameof(PartItemId))]
        public PartItem PartItem { get; set; }
    }
}
