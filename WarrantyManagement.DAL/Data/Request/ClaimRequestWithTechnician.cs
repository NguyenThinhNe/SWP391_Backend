using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Request
{
    public class ClaimRequestWithTechnician
    {
        [Required(ErrorMessage = "Technician ID is required")]
        public Guid TechnicianId { get; set; }

        [Required(ErrorMessage = "Claim request is required")]
        public ClaimRequest ClaimRequest { get; set; }
    }
}
