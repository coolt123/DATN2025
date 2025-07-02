using DATN.Dtos.ProductDto;
using DATN.Entities;

namespace DATN.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id); 
        Task<bool> UpdateProductAsync(UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<List<ProductImageDto>> GetImagesByProductIdAsync(int productId);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> SetMainImageAsync(int productId, int imageId);
        Task<ProductDto> AddProductWithImagesAsync(CreateProductWithFilesDto dto, string webRootPath);
    }
}
