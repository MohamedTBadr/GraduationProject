using Amazon.S3;
using Amazon.S3.Transfer;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace Web.Api.Controllers
{

    [ApiController]
    [Route("api/files")]
    public class FileController : BaseController
    {
        private readonly IAmazonS3 _s3;
        private readonly AwsSettings _settings;

        public FileController(IAmazonS3 s3, IOptions<AwsSettings> settings)
        {
            _s3 = s3;
            _settings = settings.Value;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required");

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

            return Ok(new { key, url });
        }
    }

}
