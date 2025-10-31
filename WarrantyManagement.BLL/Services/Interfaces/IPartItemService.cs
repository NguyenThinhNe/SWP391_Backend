using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Entities;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IPartItemService
    {
        /// <summary>
        /// Lấy danh sách PartItem có phân trang và lọc
        /// </summary>
        Task<IEnumerable<PartItemDto>> GetPartItemsAsync(PartItemDto dto);

        /// <summary>
        /// Lấy thông tin chi tiết của một PartItem
        /// </summary>
        Task<PartItemDetailDto> GetPartItemByIdAsync(Guid partItemId);

        /// <summary>
        /// Tạo mới PartItem
        /// </summary>
        Task<PartItemDto> CreatePartItemAsync(CreatePartItemDto createDto);

        /// <summary>
        /// Cập nhật PartItem
        /// </summary>
        Task<PartItemDto> UpdatePartItemAsync(Guid partItemId, UpdatePartItemDto updateDto);

        /// <summary>
        /// Xóa PartItem
        /// </summary>
        Task<bool> DeletePartItemAsync(Guid partItemId);

        /// <summary>
        /// Lấy danh sách PartItem theo PartId
        /// </summary>
        Task<IEnumerable<PartItemDto>> GetPartItemsByPartIdAsync(Guid partId);

        /// <summary>
        /// Lấy danh sách PartItem theo InventoryId
        /// </summary>
        Task<IEnumerable<PartItemDto>> GetPartItemsByInventoryIdAsync(Guid inventoryId);

        /// <summary>
        /// Lấy danh sách PartItem theo ClaimId
        /// </summary>
        Task<IEnumerable<PartItemDto>> GetPartItemsByClaimIdAsync(Guid claimId);

        /// <summary>
        /// Kiểm tra PartItem có tồn tại không
        /// </summary>
        Task<bool> PartItemExistsAsync(Guid partItemId);

        /// <summary>
        /// Kiểm tra PartNumber đã tồn tại chưa
        /// </summary>
        Task<bool> PartNumberExistsAsync(string partNumber, Guid? excludePartItemId = null);
        Task HandleClaimPartItemsAsync(ClaimRequest request, WarrantyClaim claim);
    }
}
