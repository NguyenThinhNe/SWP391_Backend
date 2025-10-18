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
        public Guid ClaimId;
        public string VehicleName;
        public string VIN;
        public WarrantyClaimStatus ClaimStatus;

    }
}
