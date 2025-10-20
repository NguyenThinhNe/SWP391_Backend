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

        [Required(ErrorMessage = "Service center ID is required")]
        public Guid ServiceCenterId { get; set; }

        [Required(ErrorMessage = "VIN is required")]
        public string VIN { get; set; }

        [Required(ErrorMessage = "Vehicle name is required")]
        [MaxLength(200, ErrorMessage = "Vehicle name cannot exceed 200 characters")]
        public string VehicleName { get; set; }

        [Required(ErrorMessage = "Purchase date is required")]
        public DateTime PurchaseDate { get; set; }

        [Required(ErrorMessage = "Mileage is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Mileage must be greater than or equal to 0")]
        public int Mileage { get; set; }

        [Required(ErrorMessage = "Part name is required")]
        [MaxLength(200, ErrorMessage = "Part name cannot exceed 200 characters")]
        public string PartName { get; set; }

        [Required(ErrorMessage = "Part number is required")]
        [MaxLength(100, ErrorMessage = "Part number cannot exceed 100 characters")]
        public string PartNumber { get; set; }

        [Required(ErrorMessage = "Issue description is required")]
        [MaxLength(200, ErrorMessage = "Issue description cannot exceed 200 characters")]
        public string IssueDescription { get; set; }

        [MaxLength(200, ErrorMessage = "Claim description cannot exceed 200 characters")]
        public string ClaimDescription { get; set; }

        [Required(ErrorMessage = "Policy ID is required")]
        public Guid PolicyId { get; set; }
    }
}
