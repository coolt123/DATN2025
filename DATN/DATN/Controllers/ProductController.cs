using DATN.Dtos;
using DATN.Dtos.ProductDto;
using DATN.Entities;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        [HttpGet("New")]
        public async Task<IActionResult> GetLatestProducts([FromQuery] int count = 10)
        {
            var products = await _productService.GetLatestProductsAsync(count);
            return ResponseHelper.ResponseSuccess(products);
        }
        [HttpGet("Seller")]
        public async Task<IActionResult> Getsell([FromQuery] int count )
        {
            var products = await _productService.Getproductseller(count);
            return ResponseHelper.ResponseSuccess(products);
        }
        [HttpGet("products")]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams query)
        {
            var paged = await _productService.GetAllProductsAsyncsearch(query);

            return ResponseHelper.ResponseSuccess(
                data: paged.Items,
                message: "Lấy danh sách sản phẩm thành công",
                meta: paged.Meta);
        }
        [HttpGet("products/admin")]
        public async Task<IActionResult> GetAllAdmin([FromQuery] ProductQueryParams query)
        {
            var paged = await _productService.GetAllProductsAsyncsearchAdmin(query);

            return ResponseHelper.ResponseSuccess(
                data: paged.Items,
                message: "Lấy danh sách sản phẩm thành công",
                meta: paged.Meta);
        }
        [HttpGet("product/search")]
        public async Task<IActionResult> GetSearch([FromQuery] ProductQueryParams query)
        {
            var paged = await _productService.GetAllProductsAsyncpagesearch(query);
            return ResponseHelper.ResponseSuccess(
                data: paged.Items,
                message: "Lấy danh sách sản phẩm thành công",
                meta: paged.Meta);
        }
        [HttpPost("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            var product = await _productService.RestoreProductAsync(id);
            return ResponseHelper.ResponseSuccess(
                data: product,
                message: "Khôi phục sản phẩm thành công", 
                meta: null);
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
        [Consumes("multipart/form-data")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> CreateWithImages([FromForm] CreateProductWithFilesDto dto)
        {
            var webRootPath = _env.WebRootPath;
            var product = await _productService.AddProductWithImagesAsync(dto, webRootPath);

            return new ObjectResult(new ResponseDto<ProductDto>
            {
                statusCode = (int)HttpStatusCode.Created,
                Message = "Thêm sản phẩm thành công",
                Data = product
            })
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }
        [Authorize(Roles = AppRole.Admin)]
        [Consumes("multipart/form-data")]
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportProductsFromExcel([FromForm] ImportProductsRequest request)
        {
            var webRootPath = _env.WebRootPath;
            await _productService.ImportProductsFromExcelAndImagesAsync(request.ExcelFile, request.Images,webRootPath);
            return Ok("Sản phẩm được import thành công");
        }
        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Update([FromForm] int id , UpdateProductWithFilesDto dto)
        {
            var webRootPath = _env.WebRootPath;
            var result = await _productService.UpdateProductWithImagesAsync(id,dto,webRootPath);
            if (!result)
                return ResponseHelper.ResponseError("Không tìm thấy sản phẩm để cập nhật", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(result, "Cập nhật sản phẩm thành công");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return ResponseHelper.ResponseError("Không tìm thấy sản phẩm để xoá", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess<string>(null, "Xoá sản phẩm thành công");
        }
        [HttpDelete("delete-multiple")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> DeleteList([FromBody] int[] id)
        {
            var result = await _productService.DeleteListProductAsync(id);
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
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var result = await _productService.DeleteImageAsync(imageId);
            if (!result)
                return ResponseHelper.ResponseError("Không tìm thấy ảnh để xoá", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess<string>(null, "Xoá ảnh thành công");
        }

        [HttpPut("{productId}/set-main-image/{imageId}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> SetMainImage(int productId, int imageId)
        {
            var result = await _productService.SetMainImageAsync(productId, imageId);
            if (!result)
                return ResponseHelper.ResponseError("Không thể đặt ảnh chính", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<string>(null, "Cập nhật ảnh chính thành công");
        }
    }
}
