using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Response
{
    public class CampaignResponse
    {
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public CampaignStatus Status { get; set; } 
        public Guid? TechnicanId { get; set; }
        public Guid? ServiceCenterId { get; set; }
        public List<VehicleBasicInfo> Vehicles { get; set; } = new List<VehicleBasicInfo>();

    }
    public class VehicleBasicInfo
    {
        public string Vin { get; set; }
        public string VehicleName { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int MileAge { get; set; }
    }
}
