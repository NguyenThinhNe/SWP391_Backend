using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class Customer
    {
        [Key]
        public Guid CustomerId { get; set; }
        
        [MaxLength(50)]
        public string FirstName { get; set; }
        
        [MaxLength(50)]
        public string LastName { get; set; }
        
        [MaxLength(10)]
        public string PhoneNumber { get; set; }
        
        public string Email { get; set; }

        public ICollection<CustomerVehicle> CustomerVehicles { get; set; }
    }
}
