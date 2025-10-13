using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Data.Entities
{
    public class Report
    {
        public Guid ReportId { get; set; }
        public string Description { get; set; }
        public double TotalPrice { get; set; }
        public DateTime CreatedTime { get; set; }
        public Guid TypeId { get; set; }
        public Guid PolicyId { get; set; }
        public Guid UserId { get; set; }
        public Guid CustomerId { get; set; }

        public enum Status
        {
            Pending,
            InProgress,
            Completed,
            Overdue
        }
    }
}
