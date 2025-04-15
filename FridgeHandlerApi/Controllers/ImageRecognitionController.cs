using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using FridgeHandler.Services.Interface;

namespace FridgeHandlerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageRecognitionController : ControllerBase
    {
        private readonly ILogger<ImageRecognitionController> _logger;
        private readonly IImageRecognitionService _imageRecognitionService;

        public ImageRecognitionController(ILogger<ImageRecognitionController> logger,
            IImageRecognitionService imageRecognitionService)
        {
            _logger = logger;
            _imageRecognitionService = imageRecognitionService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("No image uploaded.");

            try
            {
                var result = await _imageRecognitionService.AnalyzeImageAsync(imageFile);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing image: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}