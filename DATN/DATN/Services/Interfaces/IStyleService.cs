using DATN.Dtos.StyleDto;

namespace DATN.Services.Interfaces
{
    public interface IStyleService
    {
        Task<List<StyleDto>> GetAllAsync();
        Task<StyleDto> GetByIdAsync(int id);
        Task<bool> CreateAsync(CreateStyleDto dto);
        Task<bool> UpdateAsync(int id, UpdateStyleDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
