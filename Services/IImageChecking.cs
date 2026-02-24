using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
namespace chatApp.Services
{
    public interface IImageChecking
    {
        bool HasImageSignature(IFormFile file);
        bool IsValidImage(IFormFile file, out Image image);
        void ValidateImageConstraints(Image img);
        Stream SaveSanitizedImage(Image img);
    }
}