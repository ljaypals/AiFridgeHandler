using FridgeHandler.Data.Models;
using Microsoft.AspNetCore.Http;

namespace FridgeHandler.Services.Interface;

public interface IImageRecognitionService
{
    Task<FoodScanResult> AnalyzeImageAsync(IFormFile imageFile);
}
