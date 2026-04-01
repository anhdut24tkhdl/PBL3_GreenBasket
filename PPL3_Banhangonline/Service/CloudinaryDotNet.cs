namespace PPL3_Banhangonline.Service
{
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using PPL3_Banhangonline.Models;

    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var acc = new CloudinaryDotNet.Account(
                config["CloudinarySettings:CloudName"],
                config["CloudinarySettings:ApiKey"],
                config["CloudinarySettings:ApiSecret"]
            );
            _cloudinary = new Cloudinary(acc);
        }

        public string UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            using var stream = file.OpenReadStream();
            var upload = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream)
            };
            var result = _cloudinary.Upload(upload);
            return result.SecureUrl?.ToString();
        }
    }
}
