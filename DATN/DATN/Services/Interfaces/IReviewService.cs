using DATN.Dtos.ReviewDto;

namespace DATN.Services.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetReviewsByProductIdAsync(int productId);
        Task<ReviewDto> AddReviewAsync(string userId, ReviewCreateDto dto);
        Task<ReviewDto> UpdateReviewAsync(string userId, ReviewUpdateDto dto);
        Task<bool> DeleteReviewAsync(string userId, int reviewId);
    }
}
