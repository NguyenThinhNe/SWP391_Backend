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
        public WarrantyClaimStatus ClaimStatus { get; set; }
        public string IssueDescription { get; set; }
        public string ClaimDescription { get; set; }

        // Vehicle information
        public string VehicleName { get; set; }
        public string VIN { get; set; }
        public int Mileage { get; set; }
        public DateTime PurchaseDate { get; set; }

        // Part information
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        // Par item information
        public string PartNumber { get; set; }

        // Service Center Information
        public Guid ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; }

        // Policy Information
        public Guid PolicyId { get; set; }
        public string PolicyName { get; set; }

        // User information
        public Guid UserId { get; set; }

    }
}
