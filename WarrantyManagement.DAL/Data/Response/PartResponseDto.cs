using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Response
{
    public class PartResponseDto
    {
        public Guid PartId { get; set; }
        public string PartName { get; set; }
        public string? PartDescription { get; set; }
        public List<string> PartNumbers { get; set; }

        // --- VehiclePart Info ---
        public Guid VehiclePartId { get; set; }
        public int Quantity { get; set; }
        public string? VehiclePartDescription { get; set; }
        public bool Status { get; set; }

        // --- Customer Vehicle Info ---
        public string VIN { get; set; }
        public string? VehicleName { get; set; }
        public string? Model { get; set; }
    }
}
