using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class WarrantyValidationDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public DateTime? WarrantyStartDate { get; set; }
        public DateTime? WarrantyEndDate { get; set; }
        public int? RemainingDays { get; set; }
        public WarrantyPolicyDto Policy { get; set; }
        public PartItemDto PartItem { get; set; }
    }
}
