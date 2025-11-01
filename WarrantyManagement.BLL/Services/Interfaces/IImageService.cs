using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.BLL.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<List<string>> UploadImagesAsync(List<IFormFile> files, string folder);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<bool> DeleteImagesAsync(List<string> imageUrls);
        bool ValidateImage(IFormFile file);
    }
}
