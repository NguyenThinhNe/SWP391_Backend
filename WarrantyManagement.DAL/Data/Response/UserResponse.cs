using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;

namespace WarrantyManagement.DAL.Data.Response
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CoverImage { get; set; }
        public DateTime CreatedTime { get; set; }
        public UserRole Role { get; set; }

        // Tùy hệ thống có thể trả thêm thông tin trung tâm
        public Guid? ServiceCenterId { get; set; }
        public string? ServiceCenterName { get; set; }
    }
}
