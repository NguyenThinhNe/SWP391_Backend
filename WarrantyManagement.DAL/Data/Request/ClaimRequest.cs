using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Request
{
    public class ClaimRequest
    {
        [Required(ErrorMessage = "Claim date is required")]
        public DateTime ClaimDate { get; set; }

        [Required(ErrorMessage = "VIN is required")]
        [MaxLength(50)]
        public string VIN { get; set; }
        public string VehicleName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int Mileage { get; set; }
        [Required(ErrorMessage = "Issue description is required")]
        [MaxLength(500)]
        public string IssueDescription { get; set; }

        [MaxLength(500)]
        public string ClaimDescription { get; set; }

        [Required(ErrorMessage = "Policy ID is required")]
        public Guid PolicyId { get; set; }

        /// <summary>
        /// List of part IDs that need to be claimed
        /// Technician selects from existing parts in the system
        /// </summary>
        [Required(ErrorMessage = "At least one part must be selected")]
        [MinLength(1, ErrorMessage = "At least one part must be selected")]
        public List<PartItemRequest> PartItems { get; set; }
    }

    public class PartItemRequest
    {
        [Required(ErrorMessage = "Part ID is required")]
        public Guid PartId { get; set; }
        
        public string PartName { get; set; }
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; } = 1;
        [MaxLength(50, ErrorMessage = "Part number cannot exceed 50 characters")]
        public string PartNumber  { get; set; }
      
    }
}
