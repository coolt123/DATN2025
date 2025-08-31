// Services/Implements/StyleService.cs
using AutoMapper;
using DATN.DbContexts;
using DATN.Dtos.StyleDto;
using DATN.Entities;
using DATN.Exceptions;
using DATN.Services.Interfaces;
using Google;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services.Implements
{
    public class StyleService : IStyleService
    {
        private readonly Data _context;
        private readonly IMapper _mapper;

        public StyleService(Data context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<StyleDto>> GetAllAsync()
        {
            var styles = await _context.Styles.ToListAsync();
            return _mapper.Map<List<StyleDto>>(styles);
        }

        public async Task<StyleDto> GetByIdAsync(int id)
        {
            var style = await _context.Styles.FindAsync(id);
            return _mapper.Map<StyleDto>(style);
        }

        public async Task<bool> CreateAsync(CreateStyleDto dto)
        {
            var exists = await _context.Styles.AnyAsync(x => x.Name.ToLower() == dto.Name.ToLower());
            if (exists)
                throw new AppException("Tên phong cách đã tồn tại.", 400);
            var style = _mapper.Map<Style>(dto);
            _context.Styles.Add(style);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(int id, UpdateStyleDto dto)
        {
            var style = await _context.Styles.FindAsync(id);
            if (style == null)
                throw new AppException("Không tìm thấy phong cách.", 404);

           
            var exists = await _context.Styles
                .AnyAsync(x => x.Id != id && x.Name.ToLower() == dto.Name.ToLower());

            if (exists)
                throw new AppException("Tên phong cách đã tồn tại.", 400);

            _mapper.Map(dto, style);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var style = await _context.Styles.FindAsync(id);
            if (style == null) return false;
            bool isInUse = await _context.Products.AnyAsync(p => p.StyleId == id);
            if (isInUse)
                throw new AppException("Không thể xoá vì thể loại này đang được sử dụng cho sản phẩm.");
            _context.Styles .Remove(style);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
