using DATN.Dtos.ReviewDto;
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
    public class ReviewCotroller : ControllerBase
    {
        private readonly IReviewService _reviewservice;
        public ReviewCotroller(IReviewService reviewservice)
        {
            _reviewservice = reviewservice;

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Addreview(ReviewCreateDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _reviewservice.AddReviewAsync(userId, dto);
                return ResponseHelper.ResponseSuccess(result);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Getreview(int id)
        {
            try
            {
                var result = await _reviewservice.GetReviewsByProductIdAsync(id);
                return ResponseHelper.ResponseSuccess(result);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }



    }
}
