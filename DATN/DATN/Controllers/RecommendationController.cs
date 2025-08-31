using DATN.Dtos.ProductDto;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }
        //[HttpGet("vector/{id}")]
        //public IActionResult GetVector(int id)
        //{
        //    var vector = _recommendationService.GetVectorByProductId(id);
        //    if (vector == null)
        //        return NotFound();

        //    return Ok(new { ProductId = id, Vector = vector });
        //}

        [HttpGet("guest")]
        public async Task<IActionResult> RecommendForGuest()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = HttpContext.Session.Id; 
                HttpContext.Session.SetString("SessionId", sessionId);
            }

            var result = await _recommendationService.RecommendForGuestAsync(sessionId);
            return ResponseHelper.ResponseSuccess(result);
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> RecommendForUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _recommendationService.RecommendForUserAsync(userId);
            return ResponseHelper.ResponseSuccess(result);
          
        }
        [HttpGet("related/{productId}")]
        public async Task<IActionResult> RelatedProducts(int productId)
        {
            var result = await _recommendationService.RecommendRelatedAsync(productId);
            return ResponseHelper.ResponseSuccess(result);
        }
        [HttpGet("vector/{productId}")]
        public IActionResult GetVector(int productId)
        {
            var vector = _recommendationService.GetProductVector(productId);
            if (vector == null)
                return NotFound($"Không tìm thấy vector cho ProductId = {productId}");

            return Ok(vector);
        }
        [HttpGet("vectors/all")]
        public IActionResult GetAllVectors()
        {
            var result = _recommendationService.GetAllProductVectorInfo();
            return Ok(result);
        }
    }
}
