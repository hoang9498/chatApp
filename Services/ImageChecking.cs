using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace chatApp.Services
{
    public class ImageChecking : IImageChecking
    {
        public ImageChecking(){}
        public bool HasImageSignature(IFormFile file)
        {
            Span<byte> header = stackalloc byte[8];
            using var stream = file.OpenReadStream();
            if (stream.Read(header) < 8)
                return false;
            // PNG
            if (header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
                return true;
            // JPEG
            if (header[0] == 0xFF && header[1] == 0xD8)
                return true;
            // GIF
            if (header[0] == 'G' && header[1] == 'I' && header[2] == 'F')
                return true;
            // BMP
            if (header[0] == 'B' && header[1] == 'M')
                return true;
            // WEBP
            if (header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F')
                return true;
            return false;
        }

        public bool IsValidImage(IFormFile file, out Image image)
        {
            image = null!;

            try
            {
                using var stream = file.OpenReadStream();
                image = Image.Load(stream);
                var format = image.Metadata.DecodedImageFormat;

                // Optional: restrict formats
                if (format?.Name is not ("JPEG" or "PNG" or "GIF" or "BMP" or "WEBP"))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public Stream SaveSanitizedImage(Image img)
        {
            img.Metadata.ExifProfile = null;
            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(1024, 0),
                Mode = ResizeMode.Max
            }));
            var ms = new MemoryStream();
            img.Save(ms, new WebpEncoder());
            ms.Position = 0; // important
            return ms;
        }

        public void ValidateImageConstraints(Image img)
        {
            const int MAX_WIDTH = 8000;
            const int MAX_HEIGHT = 8000;

            if (img.Width > MAX_WIDTH || img.Height > MAX_HEIGHT)
                throw new Exception("Image resolution too large");
        }
    }
}