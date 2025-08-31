using AutoMapper;
using AutoMapper.QueryableExtensions;
using DATN.DbContexts;
using DATN.Dtos.ProductDto;
using DATN.Entities;
using DATN.Exceptions;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DATN.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly Data _context;
        private readonly string[] _allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSize = 4 * 1024 * 1024;
        private readonly IRecommendationService _recommendationService;

        private static readonly Dictionary<string, string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
            {
                { "price", "Price" },
                { "createdDate", "CreatedDate" },
                { "nameProduct", "NameProduct" },
                { "bestSeller", "BestSeller" }
            };
        public ProductService(Data context, IMapper mapper, IRecommendationService recommentservice)
        {
            _context = context;
            _mapper = mapper;
            _recommendationService = recommentservice;

        }

        public async Task<List<ProductDto>> GetAllProductsAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p=>p.Style)
                .Include(p => p.Material)
                .Include(p => p.ProductImages)
                .Include(p=>p.DescriptionDetails)
                .Include(p=>p.OrderDetails)
                .Where(p=>p.deleteflag==false)
                .ToListAsync();

            return _mapper.Map<List<ProductDto>>(products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Material)
                .Include(p => p.Style)
                .Include(p => p.ProductImages)
                .Include(p=>p.DescriptionDetails)
                .Include(p => p.OrderDetails)
                .Where(p => p.deleteflag == false)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> AddProductWithImagesAsync(CreateProductWithFilesDto dto, string webRootPath)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = new Product
                {
                    NameProduct = dto.NameProduct,
                    Description = dto.Description,
                    Price = dto.Price,
                    StockQuantity = dto.StockQuantity,
                    CategoryId = dto.CategoryId,
                    CreatedDate = DateTime.UtcNow,
                    MaterialId = dto.MaterialId,
                    StyleId = dto.StyleId,
                    ProductImages = new List<ProductImage>(),
                    DescriptionDetails = new List<ProductDescriptionDetail>(),
                    SalePrice = dto?.salePrice,
                    PromotionStart = dto?.promotionStart,
                    PromotionEnd = dto?.promotionEnd,

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
                if (dto.DescriptionDetails != null && dto.DescriptionDetails.Count > 0)
                {
                    foreach (var detailDto in dto.DescriptionDetails)
                    {
                        string detailImageUrl = null;

                        if (detailDto.ImageDesdetail != null)
                        {
                            
                            var existingImage = product.ProductImages
                                .FirstOrDefault(img => img.ImageUrl.EndsWith(detailDto.ImageDesdetail.FileName));

                            if (existingImage != null)
                            {
                                detailImageUrl = existingImage.ImageUrl;
                            }
                            else
                            {
                                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                                var ext = Path.GetExtension(detailDto.ImageDesdetail.FileName).ToLowerInvariant();
                                if (!allowedExtensions.Contains(ext))
                                    throw new AppException($"Ảnh mô tả '{detailDto.Title}' không đúng định dạng");

                                var fileName = Guid.NewGuid() + ext;
                                var folderPath = Path.Combine(webRootPath, "uploads");
                                Directory.CreateDirectory(folderPath);
                                var filePath = Path.Combine(folderPath, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await detailDto.ImageDesdetail.CopyToAsync(stream);
                                }

                                detailImageUrl = "/uploads/" + fileName;
                            }
                        }

                        var producdescrip = new ProductDescriptionDetail
                        {
                            Title = detailDto.Title,
                            Detail = detailDto.Detail,
                            ImageUrl = detailImageUrl
                        };
                        product.DescriptionDetails.Add(producdescrip);
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
                _recommendationService.UpdateProductVector(product);
                await transaction.CommitAsync();
                return _mapper.Map<ProductDto>(product);
            }
            catch (AppException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }
        public async Task ImportProductsFromExcelAndImagesAsync(IFormFile excelFile, List<IFormFile> imageFiles,string webRootPath)
        {
            
            var tempImageFolder = Path.Combine(webRootPath, "uploads", "temp");
            Directory.CreateDirectory(tempImageFolder);

            var uploadedImages = new Dictionary<string, string>(); 
            foreach (var imgFile in imageFiles)
            {
                var fileName = imgFile.FileName;
                var ext = Path.GetExtension(fileName).ToLowerInvariant();

                if (!_allowedImageExtensions.Contains(ext))
                    throw new ArgumentException($"Ảnh {fileName} không đúng định dạng.");

                if (imgFile.Length > MaxImageSize)
                    throw new ArgumentException($"Ảnh {fileName} vượt quá 2MB.");

                var saveFileName = Guid.NewGuid() + ext;
                var savePath = Path.Combine(tempImageFolder, saveFileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                await imgFile.CopyToAsync(stream);

                uploadedImages[fileName] = "/uploads/temp/" + saveFileName;
            }

            var tempFolder = Path.Combine(webRootPath, "tempExcels");
            Directory.CreateDirectory(tempFolder);

            var excelFileName = Guid.NewGuid() + Path.GetExtension(excelFile.FileName);
            var excelFilePath = Path.Combine(tempFolder, excelFileName);

            using (var stream = new FileStream(excelFilePath, FileMode.Create))
            {
                await excelFile.CopyToAsync(stream);
            }

            using var package = new ExcelPackage(new FileInfo(excelFilePath));

            var worksheet = package.Workbook.Worksheets[0];
            var colCount = worksheet.Dimension.End.Column;
            var rowCount = worksheet.Dimension.End.Row;

            var headers = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                headers.Add(worksheet.Cells[1, col].Text.Trim());
            }

            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = new Dictionary<string, string>();
                for (int col = 1; col <= colCount; col++)
                {
                    rowData[headers[col - 1]] = worksheet.Cells[row, col].Text.Trim();
                }     
                var product = new Product
                {
                    NameProduct = rowData.ContainsKey("NameProduct") ? rowData["NameProduct"] : null,
                    Description = rowData.ContainsKey("Description") ? rowData["Description"] : null,
                    Price = rowData.ContainsKey("Price") && decimal.TryParse(rowData["Price"], out var p) ? p : 0,
                    StockQuantity = rowData.ContainsKey("StockQuantity") && int.TryParse(rowData["StockQuantity"], out var q) ? q : 0,
                    CategoryId = rowData.ContainsKey("CategoryId") && int.TryParse(rowData["CategoryId"]?.ToString(), out var categoryId) ? categoryId : 0,
                    MaterialId = rowData.ContainsKey("MaterialId") && int.TryParse(rowData["MaterialId"]?.ToString(), out var materialId) ? materialId : 0,
                    StyleId = rowData.ContainsKey("StyleId") && int.TryParse(rowData["StyleId"]?.ToString(), out var styleId) ? styleId : 0,
                    SalePrice=rowData.ContainsKey("SalePrice") && decimal.TryParse(rowData["SalePrice"], out var s) ? s : 0,
                    CreatedDate = rowData.ContainsKey("CreatedDate") && DateTime.TryParse(rowData["CreatedDate"]?.ToString(), out var createdate) ? createdate : (DateTime.Now),
                    DescriptionDetails = new List<ProductDescriptionDetail>(),
                    ProductImages = new List<ProductImage>(),
                    PromotionStart = rowData.ContainsKey("PromotionStart") && DateTime.TryParse(rowData["PromotionStart"]?.ToString(), out var promoStart) ? promoStart : (DateTime?)null,
                    PromotionEnd = rowData.ContainsKey("PromotionEnd") && DateTime.TryParse(rowData["PromotionEnd"]?.ToString(), out var promoEnd) ? promoEnd : (DateTime?)null,
                };

                if (rowData.ContainsKey("Images") && !string.IsNullOrEmpty(rowData["Images"]))
                {
                    var mainImageIndex = rowData.ContainsKey("MainImageIndex") && int.TryParse(rowData["MainImageIndex"], out var mIdx) ? mIdx : 0;
                    var imageNames = rowData["Images"].Split(';', StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < imageNames.Length; i++)
                    {
                        var originalName = imageNames[i].Trim();
                        if (!uploadedImages.ContainsKey(originalName))
                            throw new ArgumentException($"Không tìm thấy ảnh {originalName} trong file upload.");

                        var imageUrlTemp = uploadedImages[originalName];
                        var ext = Path.GetExtension(imageUrlTemp).ToLowerInvariant();

                      
                        var fileNameFinal = Guid.NewGuid() + ext;
                        var finalFolder = Path.Combine(webRootPath, "uploads");
                        Directory.CreateDirectory(finalFolder);
                        var finalPath = Path.Combine(finalFolder, fileNameFinal);

                        File.Copy(Path.Combine(webRootPath, imageUrlTemp.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)), finalPath, true);

                        var img = new ProductImage
                        {
                            ImageUrl = "/uploads/" + fileNameFinal,
                            DisplayOrder = i,
                            IsMain = i == mainImageIndex
                        };
                        product.ProductImages.Add(img);
                        if (img.IsMain)
                            product.ImageUrl = img.ImageUrl;
                    }
                }
                for (int i = 1; i <= 5; i++)
                {
                    var titleKey = $"Title{i}";
                    var detailKey = $"Detail{i}";
                    var imageKey = $"ImageDesdetail{i}";

                    if (!rowData.ContainsKey(titleKey) || string.IsNullOrEmpty(rowData[titleKey]))
                        continue;

                    string detailImageUrl = null;
                    if (rowData.ContainsKey(imageKey) && !string.IsNullOrEmpty(rowData[imageKey]))
                    {
                        var originalName = rowData[imageKey].Trim();
                        if (uploadedImages.ContainsKey(originalName))
                        {
                            var imageUrlTemp = uploadedImages[originalName];
                            var ext = Path.GetExtension(imageUrlTemp).ToLowerInvariant();
                            var fileNameFinal = Guid.NewGuid() + ext;
                            var finalFolder = Path.Combine(webRootPath, "uploads");
                            Directory.CreateDirectory(finalFolder);
                            var finalPath = Path.Combine(finalFolder, fileNameFinal);

                            File.Copy(Path.Combine(webRootPath, imageUrlTemp.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)), finalPath, true);
                            detailImageUrl = "/uploads/" + fileNameFinal;
                        }
                    }

                    product.DescriptionDetails.Add(new ProductDescriptionDetail
                    {
                        Title = rowData[titleKey],
                        Detail = rowData.ContainsKey(detailKey) ? rowData[detailKey] : null,
                        ImageUrl = detailImageUrl
                    });
                }
                _context.Products.Add(product);
            }

            await _context.SaveChangesAsync();
            foreach (var product in _context.Products.Local)
            {
                _recommendationService.UpdateProductVector(product);
            }

            File.Delete(excelFilePath);
        }

        public async Task<bool> UpdateProductWithImagesAsync(int productId, UpdateProductWithFilesDto dto, string webRootPath)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.DescriptionDetails)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                    throw new AppException("Không tìm thấy sản phẩm.");

               
                product.NameProduct = dto.NameProduct;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.StockQuantity = dto.StockQuantity;
                product.CategoryId = dto.CategoryId;
                product.MaterialId = dto.MaterialId;
                product.StyleId = dto.StyleId;
                product.SalePrice = dto?.SalePrice;
                product.PromotionStart = dto?.PromotionStart;
                product.PromotionEnd = dto?.PromotionEnd;

                
                var retainUrls = dto.ImageUrls?.Select(url => url.Trim()).ToList() ?? new List<string>();

                var currentImageUrls = product.ProductImages.Select(img => img.ImageUrl.Trim()).ToList();
                bool imagesChanged = !retainUrls.SequenceEqual(currentImageUrls);
                
                if (imagesChanged || (dto.NewImages != null && dto.NewImages.Count > 0))
                {
                    var imagesToRemove = product.ProductImages
                     .Where(img => !retainUrls.Contains(img.ImageUrl, StringComparer.OrdinalIgnoreCase))
                     .ToList();

                    foreach (var img in imagesToRemove)
                    {
                        var filePath = Path.Combine(webRootPath, img.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        _context.ProductImages.Remove(img); 
                    }

                   
                    if (dto.NewImages != null && dto.NewImages.Count > 0)
                    {
                        foreach (var file in dto.NewImages)
                        {
                            if (file.Length > 2 * 1024 * 1024)
                                throw new AppException("Ảnh vượt quá kích thước cho phép (tối đa 2MB)");

                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(ext))
                                throw new AppException("Ảnh không đúng định dạng (.jpg, .jpeg, .png, .webp)");

                            var fileName = Guid.NewGuid() + ext;
                            var folderPath = Path.Combine(webRootPath, "uploads");
                            Directory.CreateDirectory(folderPath);
                            var filePath = Path.Combine(folderPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var newImage = new ProductImage
                            {
                                ImageUrl = "/uploads/" + fileName,
                                IsMain = false,
                                ProductId = product.ProductId
                            };

                            product.ProductImages.Add(newImage);
                        }
                    }
                }

                foreach (var img in product.ProductImages)
                {
                    img.IsMain = false;
                }
                var allImages = product.ProductImages.OrderBy(p => p.DisplayOrder).ToList();
                if (dto.MainImageIndex >= 0 && dto.MainImageIndex < allImages.Count)
                {
                    var mainImage = allImages[dto.MainImageIndex];
                    mainImage.IsMain = true;
                    product.ImageUrl = mainImage.ImageUrl;
                }
                else if (allImages.Count > 0)
                {
                    product.ImageUrl = product.ProductImages.FirstOrDefault(img => img.IsMain)?.ImageUrl;
                }
                else
                {
                    product.ImageUrl = ""; 
                }

                
                var orderedImages = product.ProductImages.OrderBy(img => img.DisplayOrder).ToList();
                for (int i = 0; i < orderedImages.Count; i++)
                {
                    orderedImages[i].DisplayOrder = i;
                }
                if (dto.DescriptionDetails != null && dto.DescriptionDetails.Any())
                {
                    var existingDetails = product.DescriptionDetails.ToList();

                    foreach (var detailDto in dto.DescriptionDetails)
                    {
                        if (detailDto.IdDescription.HasValue)
                        {
                            // Update mô tả cũ
                            var existing = existingDetails.FirstOrDefault(d => d.IdDescription == detailDto.IdDescription.Value);
                            if (existing != null)
                            {
                                existing.Title = detailDto.Title;
                                existing.Detail = detailDto.Detail;

                                if (detailDto.NewImage != null)
                                {
                                    // Xóa ảnh cũ nếu có
                                    if (!string.IsNullOrEmpty(existing.ImageUrl))
                                    {
                                        var oldPath = Path.Combine(webRootPath,
                                            existing.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                                        if (File.Exists(oldPath)) File.Delete(oldPath);
                                    }

                                    // Validate + lưu ảnh mới
                                    var ext = Path.GetExtension(detailDto.NewImage.FileName).ToLowerInvariant();
                                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                                    if (!allowedExtensions.Contains(ext))
                                        throw new AppException("Ảnh mô tả không đúng định dạng (.jpg, .jpeg, .png, .webp)");

                                    var fileName = Guid.NewGuid() + ext;
                                    var folderPath = Path.Combine(webRootPath, "uploads");
                                    Directory.CreateDirectory(folderPath);
                                    var filePath = Path.Combine(folderPath, fileName);

                                    using (var stream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await detailDto.NewImage.CopyToAsync(stream);
                                    }

                                    existing.ImageUrl = "/uploads/" + fileName;
                                }
                                else if (!string.IsNullOrEmpty(detailDto.ExistingImageUrl))
                                {
                                    // Giữ ảnh cũ (nếu có url)
                                    existing.ImageUrl = detailDto.ExistingImageUrl;
                                }
                            }
                        }
                        else
                        {
                            // Thêm mô tả mới
                            string? imgUrl = null;
                            if (detailDto.NewImage != null)
                            {
                                var ext = Path.GetExtension(detailDto.NewImage.FileName).ToLowerInvariant();
                                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                                if (!allowedExtensions.Contains(ext))
                                    throw new AppException("Ảnh mô tả không đúng định dạng (.jpg, .jpeg, .png, .webp)");

                                var fileName = Guid.NewGuid() + ext;
                                var folderPath = Path.Combine(webRootPath, "uploads");
                                Directory.CreateDirectory(folderPath);
                                var filePath = Path.Combine(folderPath, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await detailDto.NewImage.CopyToAsync(stream);
                                }

                                imgUrl = "/uploads/" + fileName;
                            }

                            var producdescrip = new ProductDescriptionDetail
                            {
                                Title = detailDto.Title,
                                Detail = detailDto.Detail,
                                ImageUrl = imgUrl,
                            };
                            product.DescriptionDetails.Add(producdescrip);
                        }
                    }
                    var idsFromDto = dto.DescriptionDetails
                        .Where(d => d.IdDescription.HasValue)
                        .Select(d => d.IdDescription.Value)
                        .ToList();

                    var detailsToRemove = existingDetails
                        .Where(d => !idsFromDto.Contains(d.IdDescription))
                        .ToList();

                    foreach (var d in detailsToRemove)
                    {
                        if (!string.IsNullOrEmpty(d.ImageUrl))
                        {
                            var oldPath = Path.Combine(webRootPath,
                                d.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (File.Exists(oldPath)) File.Delete(oldPath);
                        }
                        _context.productDescriptionDetails.Remove(d);
                    }
                }


                await _context.SaveChangesAsync();
                _recommendationService.UpdateProductVector(product);
                await transaction.CommitAsync();
                return true;
            }
            catch (AppException)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex) {
                await transaction.RollbackAsync();
                throw ex;
            }
        }


       
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return false;
            bool inOrder = await _context.OrderDetails.AnyAsync(od => od.ProductId == id);
            if (inOrder)
                throw new AppException("Không thể xoá vì sản phẩm đã được đặt trong đơn hàng.");
            bool inCart = await _context.Carts.AnyAsync(c => c.ProductId == id);
            if (inCart)
                throw new AppException("Không thể xoá vì sản phẩm đang nằm trong giỏ hàng của khách.");

            product.deleteflag = true;
            _context.Products.Update(product);
            _context.InventoryLogs.Add(new InventoryLog
            {
                ProductId = product.ProductId,
                LogType = "Delete",
                LogDate = DateTime.UtcNow,
                Note = $"Đã xoá sản phẩm: {product.NameProduct}"
            });

            await _context.SaveChangesAsync();
            _recommendationService.RemoveProductVector(product.ProductId);
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

        public async Task<IQueryableExtensions.PagedResult<ProductDto>> GetAllProductsAsyncsearch(ProductQueryParams query)
        {
            var now = DateTime.Now;
            var q = _context.Products
                .Where(p => !p.deleteflag)
                .Include(p => p.Category)
                .Include(p => p.Style)
                .Include(p => p.Material)
                .Include(p => p.ProductImages)
                .Include(p => p.DescriptionDetails)
                .Include(p => p.OrderDetails)
                    .ThenInclude(od => od.Order)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                q = q.ApplySearch(query.Search, p => p.NameProduct, p => p.Category.NameCategory);

            }
            if (query.PriceMin.HasValue || query.PriceMax.HasValue)
            {
                q = q.Where(p =>
                  
                    (p.SalePrice.HasValue && p.PromotionStart <= now && p.PromotionEnd >= now
                        && (!query.PriceMin.HasValue || p.SalePrice.Value >= query.PriceMin.Value)
                        && (!query.PriceMax.HasValue || p.SalePrice.Value <= query.PriceMax.Value))
                    ||
                  
                    ((!p.SalePrice.HasValue || p.PromotionStart > now || p.PromotionEnd < now)
                        && (!query.PriceMin.HasValue || p.Price >= query.PriceMin.Value)
                        && (!query.PriceMax.HasValue || p.Price <= query.PriceMax.Value))
                );
            }

           
            if (query.SortBy?.ToLower() == "price")
            {
                q = query.SortDesc
                    ? q.OrderByDescending(p => (p.SalePrice.HasValue && p.PromotionStart <= now && p.PromotionEnd >= now) ? p.SalePrice.Value : p.Price)
                    : q.OrderBy(p => (p.SalePrice.HasValue && p.PromotionStart <= now && p.PromotionEnd >= now) ? p.SalePrice.Value : p.Price);
            }
            
            else if (!string.IsNullOrWhiteSpace(query.SortBy) &&
                     AllowedSortFields.TryGetValue(query.SortBy.Trim(), out var mapped))
            {
                    q = q.ApplySort(mapped, query.SortDesc);
            }
            

            var paged = await q.ToPagedResultAsync(
                query.Page,
                query.PageSize,
                items => _mapper.Map<List<ProductDto>>(items));

            return paged;
        }
        public async Task<IQueryableExtensions.PagedResult<ProductDto>> GetAllProductsAsyncsearchAdmin(ProductQueryParams query)
        {
            var now = DateTime.Now;
            var q = _context.Products
                .Where(p => p.deleteflag==true)
                .Include(p => p.Category)
                .Include(p => p.Style)
                .Include(p => p.Material)
                .Include(p => p.ProductImages)
                .Include(p => p.DescriptionDetails)
                .Include(p => p.OrderDetails)
                    .ThenInclude(od => od.Order)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                q = q.ApplySearch(query.Search, p => p.NameProduct, p => p.Category.NameCategory);

            }
            if (query.PriceMin.HasValue || query.PriceMax.HasValue)
            {
                q = q.Where(p =>

                    (p.SalePrice.HasValue && p.PromotionStart <= now && p.PromotionEnd >= now
                        && (!query.PriceMin.HasValue || p.SalePrice.Value >= query.PriceMin.Value)
                        && (!query.PriceMax.HasValue || p.SalePrice.Value <= query.PriceMax.Value))
                    ||

                    ((!p.SalePrice.HasValue || p.PromotionStart > now || p.PromotionEnd < now)
                        && (!query.PriceMin.HasValue || p.Price >= query.PriceMin.Value)
                        && (!query.PriceMax.HasValue || p.Price <= query.PriceMax.Value))
                );
            }


            if (query.SortBy?.ToLower() == "price")
            {
                q = query.SortDesc
                    ? q.OrderByDescending(p => (p.SalePrice.HasValue && p.PromotionStart <= now && p.PromotionEnd >= now) ? p.SalePrice.Value : p.Price)
                    : q.OrderBy(p => (p.SalePrice.HasValue && p.PromotionStart <= now && p.PromotionEnd >= now) ? p.SalePrice.Value : p.Price);
            }

            else if (!string.IsNullOrWhiteSpace(query.SortBy) &&
                     AllowedSortFields.TryGetValue(query.SortBy.Trim(), out var mapped))
            {
                q = q.ApplySort(mapped, query.SortDesc);
            }


            var paged = await q.ToPagedResultAsync(
                query.Page,
                query.PageSize,
                items => _mapper.Map<List<ProductDto>>(items));

            return paged;
        }
        public async Task<bool> RestoreProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.deleteflag = false; 
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IQueryableExtensions.PagedResult<ProductDto>> GetAllProductsAsyncpagesearch(ProductQueryParams query)
        {
            var now = DateTime.Now;
            var q = _context.Products
            .Where(a=>a.deleteflag==false)
            .Include(p => p.Category)
            .Include(p => p.Style)
            .Include(p => p.Material)
            .Include(p => p.ProductImages)
             .Include(p => p.DescriptionDetails)
             .Include(p=>p.OrderDetails)
                .ThenInclude(od=>od.Order)
            .AsQueryable();

           
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                q = q.ApplyKeywordSearch(query.Search, ProductQueryExtensions.SearchMode.All);
            }
            if (query.PriceMin.HasValue)
            {
                q = q.Where(p =>
                    ((p.SalePrice.HasValue && p.SalePrice.Value > 0
                      && p.PromotionStart <= now
                      && p.PromotionEnd >= now
                      && p.SalePrice.Value >= query.PriceMin.Value)
                     || (!p.SalePrice.HasValue || p.SalePrice.Value == 0
                         || p.PromotionStart > now || p.PromotionEnd < now)
                        && p.Price >= query.PriceMin.Value));
            }
            if (query.PriceMax.HasValue)
            {
                q = q.Where(p =>
                    ((p.SalePrice.HasValue && p.SalePrice.Value > 0
                      && p.PromotionStart <= now
                      && p.PromotionEnd >= now
                      && p.SalePrice.Value <= query.PriceMax.Value)
                     || (!p.SalePrice.HasValue || p.SalePrice.Value == 0
                         || p.PromotionStart > now || p.PromotionEnd < now)
                        && p.Price <= query.PriceMax.Value));
            }
           
            if (query.TopPromotion)
            {
                q = q.Where(p => (p.SalePrice.HasValue && p.SalePrice.Value > 0
                                  && p.PromotionStart <= now
                                  && p.PromotionEnd >= now)
                             || (!p.SalePrice.HasValue || p.SalePrice.Value == 0
                                 || p.PromotionStart > now || p.PromotionEnd < now))
                     .OrderByDescending(p =>
                      (p.Price - ((p.SalePrice.HasValue && p.SalePrice.Value > 0
                     && p.PromotionStart <= now
                     && p.PromotionEnd >= now)
                    ? p.SalePrice.Value
                    : p.Price)) / p.Price);
            }

            
                string? sortProperty = null;
                if (!string.IsNullOrWhiteSpace(query.SortBy) &&
                    AllowedSortFields.TryGetValue(query.SortBy.Trim().ToLowerInvariant(), out var mapped))
                {
                    sortProperty = mapped;
                }

                q = q.ApplySort(sortProperty, query.SortDesc);
            
            
            var paged = await q.ToPagedResultAsync(
                query.Page,
                query.PageSize,
                products => _mapper.Map<List<ProductDto>>(products));

            return paged;
        }
        

        public async Task<List<ProductDto>> GetLatestProductsAsync(int count = 10)
        {
            return await _context.Products
                .Where(a => a.deleteflag == false)
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<ProductDto>> Getproductseller(int count)
        {
            return await _context.Products
                .Where(a => a.deleteflag == false)
                .OrderByDescending(p => p.OrderDetails
                .Where(od => od.Order.Status == "Complete")
                .Sum(od => (int?)od.Quantity) ?? 0)
                .Take(count)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
        public async Task<bool> DeleteListProductAsync(int[] ids)
        {
            var products = await _context.Products
                .Include(p => p.ProductImages)
                .Where(p => ids.Contains(p.ProductId) && !p.deleteflag)
                .ToListAsync();

            
            if (products.Count == 0 || products.Count != ids.Length)
                return false;
            bool inOrder = await _context.OrderDetails.AnyAsync(od => ids.Contains(od.ProductId));
            if (inOrder)
                throw new AppException("Không thể xoá vì có sản phẩm đã được đặt trong đơn hàng.");
            bool inCart = await _context.Carts.AnyAsync(c => ids.Contains(c.ProductId));
            if (inCart)
                throw new AppException("Không thể xoá vì có sản phẩm đang nằm trong giỏ hàng của khách.");
            foreach (var product in products)
            {
                product.deleteflag = true;
                product.Updatedat = DateTime.UtcNow;

                if (product.ProductImages != null)
                {
                    foreach (var img in product.ProductImages)
                    {
                        img.deleteflag = true;
                        img.Updatedat = DateTime.UtcNow;
                    }
                }
            }

            
            var logs = products.Select(p => new InventoryLog
            {
                ProductId = p.ProductId,
                LogType = "Delete",
                LogDate = DateTime.UtcNow,
                Note = $"Đã xoá mềm sản phẩm: {p.NameProduct}"
            }).ToList();

            _context.InventoryLogs.AddRange(logs);
            await _context.SaveChangesAsync();

            return true;
        }

        
    }
}
