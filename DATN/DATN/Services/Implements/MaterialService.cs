using AutoMapper;
using DATN.DbContexts;
using DATN.Dtos.MaterialDto;
using DATN.Entities;
using DATN.Exceptions;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace DATN.Services.Implements
{
    public class MaterialService : IMaterialService
    {
        private readonly Data _context;
        private readonly IMapper _mapper;

        public MaterialService(Data context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MaterialDto>> GetAllAsync()
        {
            var materials = await _context.Materials.Where(a=>a.deleteflag==false).ToListAsync();
            return _mapper.Map<IEnumerable<MaterialDto>>(materials);
        }

        public async Task<MaterialDto> GetByIdAsync(int id)
        {
            var material = await _context.Materials.Where(c => c.Id == id && c.deleteflag == false).FirstOrDefaultAsync();

            return material == null ? null : _mapper.Map<MaterialDto>(material);
        }

        public async Task<MaterialDto> CreateAsync(CreateMaterialDto dto)
        {
            bool exists = await _context.Materials
            .AnyAsync(m => m.Name.ToLower() == dto.Name.Trim().ToLower());

            if (exists)
            {
                throw new AppException($"chất liệu  '{dto.Name}'đã tồn tại.");
            }
            var material = _mapper.Map<Material>(dto);
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return _mapper.Map<MaterialDto>(material);
        }

        public async Task<bool> UpdateAsync(int id, UpdateMaterialDto dto)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return false;

            _mapper.Map(dto, material);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return false;
            bool isInUse = await _context.Products.AnyAsync(p => p.MaterialId == id);
            if (isInUse)
                throw new AppException("Không thể xoá vì thể loại này đang được sử dụng cho sản phẩm.");
            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
