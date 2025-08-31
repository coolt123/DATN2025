using DATN.Dtos.BannerDto;
using DATN.Dtos.BannerDto.BannerDto;
using DATN.Entities;

namespace DATN.Services.Interfaces
{
    public interface IBannerService
    {
        Task<IEnumerable<BannerResponse>> GetAllAsync();
        Task<BannerResponse?> GetByIdAsync(int id);
        Task<bool> CreateAsync(BannerDtos dto);
        Task<bool?> UpdateAsync(int id, BannerDtos dto);
        Task<bool> DeleteAsync(int id);
    }
}
