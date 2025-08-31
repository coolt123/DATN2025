using AutoMapper;
using DATN.DbContexts;
using DATN.Dtos.CategoriesDto;
using DATN.Entities;
using DATN.Exceptions;
using DATN.Services.Interfaces;
using Google;
using Microsoft.EntityFrameworkCore;


namespace DATN.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly Data _context;
        private readonly IMapper _mapper;

        public CategoryService(Data context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _context.Categories.Where(a=>a.deleteflag == false).ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _context.Categories.Where(c => c.CategoryId == id && c.deleteflag == false).FirstOrDefaultAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> CreateAsync(CreateCategoryDto dto)
        {
            bool exists = await _context.Categories
                .AnyAsync(m => m.NameCategory.ToLower() == dto.NameCategory.Trim().ToLower());

            if (exists)
            {
                throw new AppException($"Thể loại '{dto.NameCategory}'đã tồn tại.");
            }
            var category = _mapper.Map<Category>(dto);
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(int id , UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _mapper.Map(dto, category);
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            bool isInUse = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (isInUse)
                throw new AppException("Không thể xoá vì thể loại này đang được sử dụng cho sản phẩm.");
            _context.Categories.Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
