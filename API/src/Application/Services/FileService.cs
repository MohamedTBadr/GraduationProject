using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Application.DTOs;
using Application.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace Application.Services
{
    public class FileService: IFileService
    {
        private readonly IAmazonS3 _s3;
        private readonly AwsSettings _settings;
        public List<string> allowedExtensions = new List<string>()
        {
            ".jpg",
            ".png",
            
        };

        public const int maxAllowed = 2_000_000;


        public FileService(IAmazonS3 s3, IOptions<AwsSettings> settings)
        {
            _s3 = s3;
            _settings = settings.Value;
        }

        public async Task DeleteAsync(List<string> keys)
        {
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = _settings.BucketName,
                Objects = keys.Select(k => new KeyVersion { Key = k }).ToList()
            };

            await _s3.DeleteObjectsAsync(deleteRequest);
        }
        public async Task<string> Upload(string folderName, IFormFile file)
        {
            

            var key = $"{folderName}/{Guid.NewGuid()}_{file.FileName}";
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

       
    }
}
