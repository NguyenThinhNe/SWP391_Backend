using System;

namespace WarrantyManagement.DAL.Data.Response
{
    public class ClaimImageResponse
    {
        public Guid ImageId { get; set; }
        public string ImageUrl { get; set; }
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
    }
}
