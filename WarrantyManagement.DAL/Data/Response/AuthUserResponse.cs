using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class AuthUserResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public Guid? ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; }
        public string CoverImage { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
