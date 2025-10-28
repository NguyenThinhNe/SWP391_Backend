using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class PartItemDetailDto
    {
        public Guid PartItemId { get; set; }
        public string PartNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Thông tin Part
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public string PartDescription { get; set; }

        // Thông tin Inventory (nếu có)
        public Guid? InventoryId { get; set; }
        public string InventoryName { get; set; }

        // Thông tin Service Center (nếu có)
        public Guid? ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; }

        // Số lượng claim details sử dụng part item này
        public int ClaimDetailCount { get; set; }
    }
}
