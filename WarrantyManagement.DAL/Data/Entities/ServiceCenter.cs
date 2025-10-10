using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class ServiceCenter
    {
        [Key]
        public Guid CenterId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CenterName { get; set; }

        // Navigation property: Một ServiceCenter có nhiều Users
        public ICollection<User> Users { get; set; }
    }
}
