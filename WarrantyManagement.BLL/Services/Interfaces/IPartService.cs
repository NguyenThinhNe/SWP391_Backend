using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IPartService
    {
        Task<IEnumerable<PartResponseDto>> GetAllPartsAsync();
        Task<IEnumerable<PartResponseDto>> GetPartsByVINAsync(string vin);
        Task<PartResponseDto> GetPartByIdAsync(Guid partId);
    }
}
