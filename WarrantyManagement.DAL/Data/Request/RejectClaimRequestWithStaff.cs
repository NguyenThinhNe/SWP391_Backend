using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Request
{
    public class RejectClaimRequestWithStaff
    {
        [Required(ErrorMessage = "EVM Staff ID is required")]
        public Guid EvmStaffId { get; set; }

        [Required(ErrorMessage = "Rejection reason is required")]
        [MaxLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string RejectionReason { get; set; }
    }
}
