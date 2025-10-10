using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;

namespace WarrantyManagement.DAL.Data.Request
{
    public class CreateUserRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string CoverImage { get; set; }

        [Required]
        public UserRole Role { get; set; }

        // Chỉ có khi tạo user thuộc trung tâm dịch vụ
        public Guid? ServiceCenterId { get; set; }  
    }
}
