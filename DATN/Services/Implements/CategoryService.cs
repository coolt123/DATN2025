using AutoMapper;
using DATN.DbContexts;
using DATN.Dtos.CategoriesDto;
using DATN.Entities;
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
            var categories = await _context.Categories.ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<bool> CreateAsync(CreateCategoryDto dto)
        {
            var category = _mapper.Map<Category>(dto);
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(UpdateCategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null) return false;

            _mapper.Map(dto, category);
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            _context.Categories.Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
