using DATN.Dtos.CategoriesDto;

namespace DATN.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateCategoryDto dto);
        Task<bool> UpdateAsync(int id , UpdateCategoryDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
