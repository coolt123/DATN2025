using DATN.Dtos.ProductDto;
using DATN.Entities;

namespace DATN.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<ProductDto>> RecommendForGuestAsync(string sessionId);
        Task<List<ProductDto>> RecommendForUserAsync(string userId);
        Task<List<ProductDto>> RecommendRelatedAsync(int productId);
        void UpdateProductVector(Product product);
        void RemoveProductVector(int productId);
        double[]? GetProductVector(int productId);
        IEnumerable<object> GetAllProductVectorInfo();
    }
}
