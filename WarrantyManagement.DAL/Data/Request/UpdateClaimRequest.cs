using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Request
{
    public class UpdateClaimRequest
    {

        [Required(ErrorMessage = "Claim date is required")]
        public DateTime ClaimDate { get; set; }

        [Required(ErrorMessage = "VIN is required")]
        [MaxLength(50)]
        public string VIN { get; set; }

        [Required(ErrorMessage = "Issue description is required")]
        [MaxLength(500)]
        public string IssueDescription { get; set; }

        [MaxLength(500)]
        public string ClaimDescription { get; set; }

        [Required(ErrorMessage = "Policy ID is required")]
        public Guid PolicyId { get; set; }

        [Required(ErrorMessage = "At least one part must be selected")]
        [MinLength(1, ErrorMessage = "At least one part must be selected")]
        public List<PartItemRequest> PartItems { get; set; }
    }
}
