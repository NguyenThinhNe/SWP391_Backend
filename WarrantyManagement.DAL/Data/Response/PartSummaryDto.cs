using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class PartSummaryDto
    {
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public Guid PolicyId { get; set; }

        // Aggregated data from PartItems
        public int TotalQuantity { get; set; } // Tổng số lượng từ tất cả PartItems
        public int PartItemCount { get; set; } // Số lượng PartItem
        public List<string> PartNumbers { get; set; } // Danh sách PartNumber từ PartItems
    }
}
