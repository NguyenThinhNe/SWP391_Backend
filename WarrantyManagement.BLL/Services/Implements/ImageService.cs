using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.BLL.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
namespace WarrantyManagement.BLL.Services.Implements
{
    public class ImageService 
    {
        //public async Task<string> UploadImageAsync(IFormFile file, string folder)
        //{
        //    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        //    var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        //    Directory.CreateDirectory(dir);

        //    var fileName = $"{Guid.NewGuid()}{ext}";
        //    var path = Path.Combine(dir, fileName);

        //    using var image = await Image.LoadAsync(file.OpenReadStream());
        //    image.Mutate(x => x.Resize(new ResizeOptions
        //    {
        //        Mode = ResizeMode.Max,
        //        Size = new Size(1280, 0) // giới hạn width 1280px
        //    }));
        //    await image.SaveAsync(path);

        //    return $"/uploads/{folder}/{fileName}";
        //}
    }
}
