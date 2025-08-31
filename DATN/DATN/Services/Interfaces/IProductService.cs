using DATN.Dtos.ProductDto;
using DATN.Entities;
using static IQueryableExtensions;

namespace DATN.Services.Interfaces
{
    public class ProductQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public bool TopPromotion { get; set; }

    }

    public interface IProductService
    {
        Task<List<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id); 
        Task<bool> UpdateProductWithImagesAsync(int productId, UpdateProductWithFilesDto dto, string webRootPath);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> DeleteListProductAsync(int[] ids);
        Task<List<ProductDto>> GetLatestProductsAsync(int count = 10);
        Task<List<ProductDto>> Getproductseller(int count);

        Task<List<ProductImageDto>> GetImagesByProductIdAsync(int productId);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> SetMainImageAsync(int productId, int imageId);
        Task<ProductDto> AddProductWithImagesAsync(CreateProductWithFilesDto dto, string webRootPath);
        Task ImportProductsFromExcelAndImagesAsync(IFormFile excelFile, List<IFormFile> imageFiles, string webRootPath);
        Task<IQueryableExtensions.PagedResult<ProductDto>> GetAllProductsAsyncsearch(ProductQueryParams query);
        Task<IQueryableExtensions.PagedResult<ProductDto>> GetAllProductsAsyncsearchAdmin(ProductQueryParams query);
        Task<IQueryableExtensions.PagedResult<ProductDto>> GetAllProductsAsyncpagesearch(ProductQueryParams query);
        Task<bool> RestoreProductAsync(int productId);


    }
}
