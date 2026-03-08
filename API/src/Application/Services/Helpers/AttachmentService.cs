using Amazon.S3;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Helpers
{
    public class AttachmentService : IAttachmentService
    {


        private readonly IAmazonS3 _s3;
        private readonly AwsSettings _settings;

        public AttachmentService(IAmazonS3 s3, IOptions<AwsSettings> settings)
        {
            _s3 = s3;
            _settings = settings.Value;
        }

        public List<string> allowedExtensions = new List<string>()
        {
            ".jpg",
            ".png",
            ".pdf"
        };

        public const int maxAllowed = 2_000_000;



        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {




            var key = $"uploads/{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _settings.BucketName,
                ContentType = file.ContentType
            };

            var transferUtility = new TransferUtility(_s3);
            await transferUtility.UploadAsync(uploadRequest);

            var url = $"https://{_settings.BucketName}.s3.amazonaws.com/{key}";
  

            return url;


        }

 
public async Task DeleteFileAsync(string key)
    {
        var client = new AmazonS3Client(
            _settings.AccessKey,
            _settings.SecretKey,
            Amazon.RegionEndpoint.EUWest1
        );

        var request = new DeleteObjectRequest
        {
            BucketName = "graduation-project-api-bucket",
            Key = key
        };

        await client.DeleteObjectAsync(request);
    }


}
}
