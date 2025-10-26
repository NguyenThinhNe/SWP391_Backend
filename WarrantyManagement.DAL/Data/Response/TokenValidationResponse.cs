using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class TokenValidationResponse
    {
        public bool IsValid { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateTime? Expiration { get; set; }
    }
}
