using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IWarrantyValidationService
    {
        Task<WarrantyValidationDto> ValidatePolicyForPartItemAsync(Guid policyId, Guid partItemId);
        Task<WarrantyValidationDto> CheckWarrantyStatusAsync(Guid partItemId);
        Task<IEnumerable<WarrantyValidationDto>> CheckMultiplePartItemsAsync(IEnumerable<Guid> partItemIds);
        Task<IEnumerable<WarrantyValidationDto>> GetExpiringWarrantiesAsync(int daysThreshold = 30);
    }
}
