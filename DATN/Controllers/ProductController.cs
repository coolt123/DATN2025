using DATN.Dtos;
using DATN.Dtos.ProductDto;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
            return ResponseHelper.ResponseSuccess(result, "Lấy danh sách sản phẩm thành công");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return ResponseHelper.ResponseError("Không tìm thấy sản phẩm", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(product, "Lấy chi tiết sản phẩm thành công");
        }

        [HttpPost("with-images")]
        public async Task<IActionResult> CreateWithImages([FromForm] CreateProductWithFilesDto dto)
        {
            var webRootPath = _env.WebRootPath;
            var product = await _productService.AddProductWithImagesAsync(dto, webRootPath);

            return new ObjectResult(new ResponseDto<ProductDto>
            {
                Status = (int)HttpStatusCode.Created,
                Message = "Thêm sản phẩm thành công",
                Data = product
            })
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }

        [HttpPut]
        public async Task<IActionResult> Update(UpdateProductDto dto)
        {
            var result = await _productService.UpdateProductAsync(dto);
            if (!result)
                return ResponseHelper.ResponseError("Không tìm thấy sản phẩm để cập nhật", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess<string>(null, "Cập nhật sản phẩm thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return ResponseHelper.ResponseError("Không tìm thấy sản phẩm để xoá", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess<string>(null, "Xoá sản phẩm thành công");
        }

        [HttpGet("{productId}/images")]
        public async Task<IActionResult> GetImages(int productId)
        {
            var images = await _productService.GetImagesByProductIdAsync(productId);
            return ResponseHelper.ResponseSuccess(images, "Lấy danh sách ảnh thành công");
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var result = await _productService.DeleteImageAsync(imageId);
            if (!result)
                return ResponseHelper.ResponseError("Không tìm thấy ảnh để xoá", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess<string>(null, "Xoá ảnh thành công");
        }

        [HttpPut("{productId}/set-main-image/{imageId}")]
        public async Task<IActionResult> SetMainImage(int productId, int imageId)
        {
            var result = await _productService.SetMainImageAsync(productId, imageId);
            if (!result)
                return ResponseHelper.ResponseError("Không thể đặt ảnh chính", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<string>(null, "Cập nhật ảnh chính thành công");
        }
    }
}
