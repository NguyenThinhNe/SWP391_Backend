using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarrantyManagement.BLL.Services.Interfaces;
using WarrantyManagement.DAL.Data.Request;
using WarrantyManagement.DAL.Data.Response;

namespace WarrantyManagement.API.Controllers
{
    [Route("api/partitems")]
    [ApiController]
    public class PartItemController : ControllerBase
    {
        private readonly IPartItemService _partItemService;

        public PartItemController(IPartItemService partItemService)
        {
            _partItemService = partItemService;
        }
        /// <summary>
        /// Get a list of part items
        /// </summary>
        /// <returns>List of part itemss</returns>
        /// <remarks>
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<PartItemDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPartItems([FromQuery] PartItemDto dto)
        {
            try
            {
                var result = await _partItemService.GetPartItemsAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving part items.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một PartItem
        /// </summary>
        /// <param name="id">ID của PartItem</param>
        /// <returns>Thông tin chi tiết của PartItem</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PartItemDetailDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPartItemById(Guid id)
        {
            try
            {
                var result = await _partItemService.GetPartItemByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the part item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo mới PartItem
        /// </summary>
        /// <param name="createDto">Thông tin PartItem cần tạo</param>
        /// <returns>PartItem đã được tạo</returns>
        /// <remarks>
        /// PartNumber là tùy chọn - có thể để trống
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(PartItemDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreatePartItem([FromBody] CreatePartItemDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _partItemService.CreatePartItemAsync(createDto);
                return CreatedAtAction(nameof(GetPartItemById), new { id = result.PartItemId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the part item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật PartItem
        /// </summary>
        /// <param name="id">ID của PartItem cần cập nhật</param>
        /// <param name="updateDto">Thông tin cập nhật</param>
        /// <returns>PartItem đã được cập nhật</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PartItemDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdatePartItem(Guid id, [FromBody] UpdatePartItemDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _partItemService.UpdatePartItemAsync(id, updateDto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the part item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa PartItem
        /// </summary>
        /// <param name="id">ID của PartItem cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeletePartItem(Guid id)
        {
            try
            {
                var result = await _partItemService.DeletePartItemAsync(id);
                if (result)
                    return NoContent();

                return BadRequest(new { message = "Failed to delete part item." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the part item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách PartItem theo PartId
        /// </summary>
        /// <param name="partId">ID của Part</param>
        /// <returns>Danh sách PartItem</returns>
        [HttpGet("by-part/{partId}")]
        [ProducesResponseType(typeof(IEnumerable<PartItemDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPartItemsByPartId(Guid partId)
        {
            try
            {
                var result = await _partItemService.GetPartItemsByPartIdAsync(partId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving part items by part.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách PartItem theo InventoryId
        /// </summary>
        /// <param name="inventoryId">ID của Inventory</param>
        /// <returns>Danh sách PartItem</returns>
        [HttpGet("by-inventory/{inventoryId}")]
        [ProducesResponseType(typeof(IEnumerable<PartItemDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPartItemsByInventoryId(Guid inventoryId)
        {
            try
            {
                var result = await _partItemService.GetPartItemsByInventoryIdAsync(inventoryId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving part items by inventory.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách PartItem theo ClaimId
        /// </summary>
        /// <param name="claimId">ID của Claim</param>
        /// <returns>Danh sách PartItem</returns>
        [HttpGet("by-claim/{claimId}")]
        [ProducesResponseType(typeof(IEnumerable<PartItemDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPartItemsByClaim(Guid claimId)
        {
            try
            {
                var result = await _partItemService.GetPartItemsByClaimIdAsync(claimId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving part items by claim.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra PartItem có tồn tại không
        /// </summary>
        /// <param name="id">ID của PartItem</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PartItemExists(Guid id)
        {
            try
            {
                var exists = await _partItemService.PartItemExistsAsync(id);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking part item existence.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra PartNumber đã tồn tại chưa
        /// </summary>
        /// <param name="partNumber">Part number cần kiểm tra</param>
        /// <param name="excludeId">ID của PartItem cần loại trừ (dùng khi update)</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        [HttpGet("partnumber-exists")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PartNumberExists([FromQuery] string partNumber, [FromQuery] Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(partNumber))
                return BadRequest(new { message = "Part number is required." });

            try
            {
                var exists = await _partItemService.PartNumberExistsAsync(partNumber, excludeId);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking part number existence.", error = ex.Message });
            }
        }
    }
}
