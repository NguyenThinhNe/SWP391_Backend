using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class VehicleResponse
    {
        // Thông tin CustomerVehicle
        public string VIN { get; set; }
        public string VehicleName { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int MileAge { get; set; }

        // Thông tin Customer
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        // (Tùy chọn) Thuộc tính gộp tên đầy đủ cho tiện hiển thị
        public string FullName => $"{FirstName} {LastName}";
    }
}
