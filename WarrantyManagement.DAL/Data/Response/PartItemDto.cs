using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class PartItemDto
    {
        public Guid PartItemId { get; set; }
        public string PartNumber { get; set; }
        public int Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Part info
        public Guid PartId { get; set; }
        public string PartName { get; set; }

        // Claim info
        public Guid ClaimId { get; set; }
        public string ClaimNumber { get; set; }

        // Inventory info
        public Guid InventoryId { get; set; }
    }
}
