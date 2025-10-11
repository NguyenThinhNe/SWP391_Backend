using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;

namespace WarrantyManagement.DAL.Data.Request
{
    public class UpdateUserRequest
    {
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CoverImage { get; set; }

        // Cho phép admin thay đổi role hoặc service center
        public UserRole? Role { get; set; }
        public Guid? ServiceCenterId { get; set; }
    }
}

