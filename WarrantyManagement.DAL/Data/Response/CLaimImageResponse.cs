using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class CLaimImageResponse
    {
        public Guid ImageId { get; set; }
        public string ImageUrl { get; set; }
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
    }
}
