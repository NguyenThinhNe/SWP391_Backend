using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

namespace WarrantyManagement.DAL.Data.Request
{
    public class UpdateClaimPartItemsRequest
    {

        [Required]
        public string VIN { get; set; }

        [Required]
        public ClaimActionType ActionType { get; set; }

        [Required]
        [MinLength(1)]
        public List<PartItemRequest> PartItems { get; set; }
    }
}
