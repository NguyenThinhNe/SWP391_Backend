using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Request
{
    public class ClaimRequest
    {
        [Required(ErrorMessage = "Claim date is required")]
        public DateTime ClaimDate { get; set; }
        public string CenterName { get; set; }  
        [Required(ErrorMessage = "VIN is required")]
        [MaxLength(50)]
        public string VIN { get; set; }
        public string VehicleName { get; set; }
        public int Mileage { get; set; }
        public DateTime PurchaseDate { get; set; }
        [Required(ErrorMessage = "Issue description is required")]
        [MaxLength(500)]
        public string IssueDescription { get; set; }

        /// <summary>
        /// List of part IDs that need to be claimed
        /// Technician selects from existing parts in the system
        /// </summary>
        [Required(ErrorMessage = "At least one part must be selected")]
        [MinLength(1, ErrorMessage = "At least one part must be selected")]
        public List<PartItemRequest> PartItems { get; set; }
        public ClaimActionType ActionType { get; set; }
    }

    public class PartItemRequest
    {
        public string PartName { get; set; }

        public string PartNumber  { get; set; }
        public DateTime ReplacementDate { get; set; }
    }
}
