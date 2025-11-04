using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Enums;

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

        [MaxLength(10)]
        public string PhoneNumber { get; set; }
        
        public string CoverImage { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public string Name { get; set; }
        public UserRole Role { get; set; } = UserRole.SCTech;
        public bool IsActive { get; set; } = true;

        // Foreign key reference
        public Guid? ServiceCenterId { get; set; }
        [ForeignKey(nameof(ServiceCenterId))]
        public ServiceCenter ServiceCenter { get; set; }

        public ICollection<WarrantyClaim> WarrantyClaims { get; set; }
        public ICollection<Report> Reports { get; set; }
        public Campaign? Campaign { get; set; }
    }
}
