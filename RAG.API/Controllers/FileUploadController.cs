using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RAG.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IConfiguration _config;

        public FileUploadController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var blobClient = new BlobContainerClient(_config["Azure:BlobStorageConnectionString"], _config["Azure:BlobContainer"]);
            await blobClient.CreateIfNotExistsAsync();

            var blob = blobClient.GetBlobClient(file.FileName);
            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, overwrite: true);

            return Ok(new { file.FileName });
        }
    }
}
