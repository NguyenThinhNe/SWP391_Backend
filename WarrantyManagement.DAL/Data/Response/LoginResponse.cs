using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;

namespace WarrantyManagement.DAL.Data.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public UserRole Role { get; set; }
    }
}
