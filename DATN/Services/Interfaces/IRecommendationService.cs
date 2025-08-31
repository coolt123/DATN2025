using DATN.Dtos.ProductDto;

namespace DATN.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<ProductDto>> RecommendForGuestAsync(string sessionId);
        Task<List<ProductDto>> RecommendForUserAsync(string userId);
    }
}
