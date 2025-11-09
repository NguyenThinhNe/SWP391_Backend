using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class WarrantyPolicyDto
    {
        public Guid PolicyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMonth { get; set; }
        public string Condition { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
