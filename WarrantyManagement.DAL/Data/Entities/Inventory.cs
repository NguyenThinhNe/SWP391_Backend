using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class Inventory
    {
        public Guid PartItemId { get; set; }
        public string PartName { get; set; }
        public Guid CenterId { get; set; }
    }
}
