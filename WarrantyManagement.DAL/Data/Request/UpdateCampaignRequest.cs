using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Request
{
    public class UpdateCampaignRequest
    {
        public string CampaignName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } 
        public CampaignStatus Status { get; set; }

    }
}
