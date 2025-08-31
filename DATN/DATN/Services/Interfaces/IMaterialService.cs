using DATN.Dtos.MaterialDto;

namespace DATN.Services.Interfaces
{
    public interface IMaterialService
    {
        Task<IEnumerable<MaterialDto>> GetAllAsync();
        Task<MaterialDto> GetByIdAsync(int id);
        Task<MaterialDto> CreateAsync(CreateMaterialDto dto);
        Task<bool> UpdateAsync(int id, UpdateMaterialDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
