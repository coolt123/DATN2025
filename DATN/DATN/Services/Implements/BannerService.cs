using DATN.DbContexts;
using DATN.Dtos.BannerDto;
using DATN.Dtos.BannerDto.BannerDto;
using DATN.Entities;
using DATN.Services.Interfaces;
using Google;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services.Implements
{
    public class BannerService : IBannerService
    {
        private readonly Data _context;
        private readonly IWebHostEnvironment _env;

        public BannerService(Data context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<BannerResponse>> GetAllAsync()
        {
            var banners = await _context.Banners
                 .Include(x => x.Category)
                 .ToListAsync();
            return banners.Select(b => new BannerResponse
            {
                BannerId = b.BannerId,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                CategoryId = b.CategoryId,
                LinkUrl=b.LinkUrl,
                CategoryName = b.Category?.NameCategory
            });
        }

        public async Task<BannerResponse?> GetByIdAsync(int id)
        {
            var banner = await _context.Banners
                .Where(b => b.BannerId == id)
                .Select(b => new BannerResponse
                {
                    BannerId = b.BannerId,
                    Title = b.Title,
                    ImageUrl=b.ImageUrl,
                    LinkUrl = b.LinkUrl,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category != null ? b.Category.NameCategory : null
                })
                .FirstOrDefaultAsync();

            if (banner == null)
            {
                throw new ApplicationException($"Banner with ID {id} not found.");
            }

            return banner;
        }

        public async Task<bool> CreateAsync(BannerDtos dto)
        {
            try
            {
                var banner = new Banner
                {
                    Title = dto.Title,
                    LinkUrl = dto.CategoryId.HasValue ? $"/category/{dto.CategoryId}" : null,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = dto.IsActive,
                    CategoryId = dto.CategoryId,
                    Createdat = DateTime.UtcNow,
                };

                if (dto.Image != null)
                {
                    banner.ImageUrl = await SaveImageAsync(dto.Image);
                }

                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool?> UpdateAsync(int id, BannerDtos dto)
        {
            try
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner == null) return false;

                banner.Title = dto.Title;
                banner.LinkUrl = dto.CategoryId.HasValue ? $"/category/{dto.CategoryId}" : null;
                banner.DisplayOrder = dto.DisplayOrder;
                banner.IsActive = dto.IsActive;
                banner.Updatedat = DateTime.UtcNow;
                banner.CategoryId = dto.CategoryId;

                if (dto.Image != null)
                {
                    banner.ImageUrl = await SaveImageAsync(dto.Image);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return false;

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }
    }

}
