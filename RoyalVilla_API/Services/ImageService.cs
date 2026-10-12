using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Services
{
    public class ImageService : IImageService
    {

        private const long MaxFileSize = 5 * 1024 * 1024; //5MB

        private readonly string[] AllowedExtension = { ".jpg", ".jpeg", ".png" };

        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return false;
                }
                var fileName = Path.GetFileName(imageUrl);
                //access the wwwroot folder we use webhost.webroot 
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "villas", fileName);

                //folder not exist create
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                //validate the file
                if (!ValidateImage(file)) {
                    throw new InvalidOperationException("Invalid image file");
                }

                //access the wwwroot folder we use webhost.webroot 
                var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "villas");

                //folder not exist create
                if(!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var fileExtension = Path.GetExtension(file.FileName);

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                //save file

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/images/villas/{uniqueFileName}";
            }
            catch (Exception ex) {
                throw;
            }
        }

        public bool ValidateImage(IFormFile file)
        {
            if(file==null ||file.Length ==0)
            {
                return false;
            }

            if(file.Length > MaxFileSize)
            {
                return false;
            }
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if(!AllowedExtension.Contains(extension))
            {
                return false;
            }
            return true;
        }
    }
}
