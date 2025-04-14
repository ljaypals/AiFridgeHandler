// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using System.IO;
// using System.Threading.Tasks;
//
// namespace FridgeHandlerAPI.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class ImageRecognitionController : ControllerBase
//     {
//         private readonly ILogger<ImageRecognitionController> _logger;
//
//         public ImageRecognitionController(ILogger<ImageRecognitionController> logger)
//         {
//             _logger = logger;
//         }
//
//         [HttpPost("upload")]
//         [Consumes("multipart/form-data")] // ✅ Required for Swagger compatibility
//         public async Task<IActionResult> UploadImage([FromForm] IFormFile imageFile)
//         {
//             if (imageFile == null || imageFile.Length == 0)
//                 return BadRequest("No image uploaded.");
//
//             try
//             {
//                 var uploadsDir = Path.Combine("wwwroot", "uploads");
//                 if (!Directory.Exists(uploadsDir))
//                     Directory.CreateDirectory(uploadsDir);
//
//                 var filePath = Path.Combine(uploadsDir, imageFile.FileName);
//
//                 using (var stream = new FileStream(filePath, FileMode.Create))
//                 {
//                     await imageFile.CopyToAsync(stream);
//                 }
//
//                 // TODO: Integrate with AI Image Recognition API (e.g., Google Vision API)
//
//                 return Ok(new { message = "Image uploaded successfully", filePath });
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error uploading image: {ex.Message}");
//                 return StatusCode(500, "Internal Server Error");
//             }
//         }
//     }
// }