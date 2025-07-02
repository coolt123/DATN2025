using AutoMapper;
using DATN.DbContexts;
using DATN.Dtos.ProductDto;
using DATN.Entities;
using DATN.Exceptions;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly Data _context;

        public ProductService(Data context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> AddProductWithImagesAsync(CreateProductWithFilesDto dto, string webRootPath)
        {
            var product = new Product
            {
                NameProduct = dto.NameProduct,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                CreatedDate = DateTime.UtcNow,
                ProductImages = new List<ProductImage>()
            };

            if (dto.Images != null && dto.Images.Count > 0)
            {
                for (int i = 0; i < dto.Images.Count; i++)
                {
                    var file = dto.Images[i];

                    if (file.Length > 2 * 1024 * 1024)
                        throw new AppException($"Ảnh thứ {i + 1} vượt quá kích thước cho phép (tối đa 2MB)");

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                        throw new AppException($"Ảnh thứ {i + 1} không đúng định dạng (.jpg, .png, .webp)");

                    var fileName = Guid.NewGuid() + ext;
                    var folderPath = Path.Combine(webRootPath, "uploads");
                    Directory.CreateDirectory(folderPath);
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var image = new ProductImage
                    {
                        ImageUrl = "/uploads/" + fileName,
                        DisplayOrder = i,
                        IsMain = i == dto.MainImageIndex
                    };

                    product.ProductImages.Add(image);
                    if (image.IsMain)
                        product.ImageUrl = image.ImageUrl;
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            _context.InventoryLogs.Add(new InventoryLog
            {
                ProductId = product.ProductId,
                ChangeQuantity = product.StockQuantity,
                LogType = "Create",
                LogDate = DateTime.UtcNow,
                Note = $"Đã thêm sản phẩm: {product.NameProduct}"
            });

            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> UpdateProductAsync(UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return false;

            _mapper.Map(dto, product);
            await _context.SaveChangesAsync();

            _context.InventoryLogs.Add(new InventoryLog
            {
                ProductId = product.ProductId,
                LogType = "Update",
                LogDate = DateTime.UtcNow,
                Note = $"Cập nhật sản phẩm: {product.NameProduct}"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return false;

            _context.ProductImages.RemoveRange(product.ProductImages);
            _context.Products.Remove(product);

            _context.InventoryLogs.Add(new InventoryLog
            {
                ProductId = product.ProductId,
                LogType = "Delete",
                LogDate = DateTime.UtcNow,
                Note = $"Đã xoá sản phẩm: {product.NameProduct}"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductImageDto>> GetImagesByProductIdAsync(int productId)
        {
            var images = await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return _mapper.Map<List<ProductImageDto>>(images);
        }

        public async Task<bool> DeleteImageAsync(int imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null) return false;

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetMainImageAsync(int productId, int imageId)
        {
            var images = await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .ToListAsync();

            var selected = images.FirstOrDefault(i => i.ProductImageId == imageId);
            if (selected == null) return false;

            foreach (var image in images)
                image.IsMain = image.ProductImageId == imageId;

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
                product.ImageUrl = selected.ImageUrl;

            await _context.SaveChangesAsync();
            return true;
        }

       
    }
}
