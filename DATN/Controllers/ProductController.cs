using DATN.Dtos;
using DATN.Dtos.ProductDto;
using DATN.Entities;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        public ProductController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllProductsAsync();
            return Ok(new ResponseDto<List<ProductDto>>
            {
                Data = result,
                Message = "Lấy danh sách sản phẩm thành công"
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new ResponseDto<string> { Status = 404, Message = "Không tìm thấy sản phẩm" });

            return Ok(new ResponseDto<ProductDto>
            {
                Data = product,
                Message = "Lấy chi tiết sản phẩm thành công"
            });
        }

        [HttpPost("with-images")]
        public async Task<IActionResult> CreateWithImages([FromForm] CreateProductWithFilesDto dto)
        {
            var webRootPath = _env.WebRootPath;
            var product = await _productService.AddProductWithImagesAsync(dto, webRootPath);

            return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, new ResponseDto<ProductDto>
            {
                Status = 201,
                Message = "Thêm sản phẩm thành công",
                Data = product
            });
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateProductDto dto)
        {
            var result = await _productService.UpdateProductAsync(dto);
            if (!result)
                return NotFound(new ResponseDto<string> { Status = 404, Message = "Không tìm thấy sản phẩm để cập nhật" });

            return Ok(new ResponseDto<string>
            {
                Message = "Cập nhật sản phẩm thành công"
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound(new ResponseDto<string> { Status = 404, Message = "Không tìm thấy sản phẩm để xoá" });

            return Ok(new ResponseDto<string>
            {
                Message = "Xoá sản phẩm thành công"
            });
        }

        [HttpGet("{productId}/images")]
        public async Task<IActionResult> GetImages(int productId)
        {
            var images = await _productService.GetImagesByProductIdAsync(productId);
            return Ok(new ResponseDto<List<ProductImageDto>>
            {
                Data = images,
                Message = "Lấy danh sách ảnh thành công"
            });
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var result = await _productService.DeleteImageAsync(imageId);
            if (!result)
                return NotFound(new ResponseDto<string> { Status = 404, Message = "Không tìm thấy ảnh để xoá" });

            return Ok(new ResponseDto<string> { Message = "Xoá ảnh thành công" });
        }

        [HttpPut("{productId}/set-main-image/{imageId}")]
        public async Task<IActionResult> SetMainImage(int productId, int imageId)
        {
            var result = await _productService.SetMainImageAsync(productId, imageId);
            if (!result)
                return BadRequest(new ResponseDto<string> { Status = 400, Message = "Không thể đặt ảnh chính" });

            return Ok(new ResponseDto<string> { Message = "Cập nhật ảnh chính thành công" });
        }
    }
}
