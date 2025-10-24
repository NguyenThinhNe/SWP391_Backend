using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Response
{
    public class ClaimResponse
    {


        public Guid ClaimId { get; set; }
        public DateTime ClaimDate { get; set; }
        public string VIN { get; set; }
        public WarrantyClaimStatus ClaimStatus { get; set; }
        public string IssueDescription { get; set; }
        public string ClaimDescription { get; set; }

        // Vehicle information
        public string VehicleName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int Mileage { get; set; }

        // Parts information (multiple parts)
        public List<PartItemResponse> Parts { get; set; }

        // Total cost of all parts
        public int TotalCost => Parts?.Sum(p => p.TotalCost) ?? 0;

        // Policy information
        public Guid PolicyId { get; set; }
        public string PolicyName { get; set; }

        // Service Center information
        public Guid ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; }

        // Technician information
        public Guid UserId { get; set; }
        public string TechnicianName { get; set; }

    }

    public class PartItemResponse {

        public Guid PartItemId { get; set; }
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public int Quantity { get; set; }
        public int TotalCost { get; set; }
        
    }

    

}
