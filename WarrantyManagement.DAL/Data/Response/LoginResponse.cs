using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public Guid? ServiceCenterId { get; set; }
        public string Token { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string CoverImage { get; set; }
    }
}
