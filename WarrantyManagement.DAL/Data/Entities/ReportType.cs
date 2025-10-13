using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class ReportType
    {
        public Guid TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
    }
}
