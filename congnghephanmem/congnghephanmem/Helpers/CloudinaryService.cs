using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Configuration;
using System.Web;

namespace congnghephanmem.Helpers
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService()
        {
            var cloudName = ConfigurationManager.AppSettings["CloudinaryCloudName"];
            var apiKey = ConfigurationManager.AppSettings["CloudinaryApiKey"];
            var apiSecret = ConfigurationManager.AppSettings["CloudinaryApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; 
        }

        public string UploadImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return null;
            }
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.InputStream),
            };

            var uploadResult = _cloudinary.Upload(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
    }
}