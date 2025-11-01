using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<ICollection<VehicleResponse>> GetAllAsync();
        Task<VehicleResponse> GetByVinAsync(string vin);
        Task<ICollection<VehicleResponse>> GetByCustomerNameAsync(string customerName);
        Task<ICollection<VehicleResponse>> GetByCustomerIdAsync(Guid customerId);
    }
}
