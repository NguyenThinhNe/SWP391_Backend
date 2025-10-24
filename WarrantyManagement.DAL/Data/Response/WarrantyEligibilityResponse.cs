using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class WarrantyEligibilityResponse
    {
        public string VIN { get; set; }
        public Guid PolicyId { get; set; }
        public bool IsEligible { get; set; }
        public string Reason { get; set; }
    }
}
