using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CoverImage { get; set; }
        public DateTime CreatedTime { get; set; }
        public UserRole Role { get; set; }
        // Foreign key reference
        public Guid? ServiceCenterId { get; set; }

        [ForeignKey(nameof(ServiceCenterId))]
        public ServiceCenter ServiceCenter { get; set; }
    }

    public enum UserRole
    {
       SCStaff = 1,
       SCTech = 2,
       EVMStaff = 3,
       Admin = 4
    }
}
