using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Request
{
    public class UpdateWorkOrderStatusRequest
    {
        [Required]
        public WorkOrderStatus Status { get; set; }

        public string Note { get; set; } // Ghi chú khi thay đổi status
    }
}
