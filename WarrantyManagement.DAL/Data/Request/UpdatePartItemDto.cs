using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Request
{
    public class UpdatePartItemDto
    {
        [Required(ErrorMessage = "PartItem ID is required")]
        public Guid PartItemId { get; set; }

        [MaxLength(50, ErrorMessage = "Part number cannot exceed 50 characters")]
        public string PartNumber { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Part ID is required")]
        public Guid PartId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        public Guid? InventoryId { get; set; }
    }
}
