using DATN.Dtos;
using DATN.Dtos.CategoriesDto;
using DATN.Entities;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRole.Admin)]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _categoryService.GetAllAsync();
            return ResponseHelper.ResponseSuccess(data, "Lấy danh sách danh mục thành công.");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _categoryService.GetByIdAsync(id);
            if (data == null)
                return ResponseHelper.ResponseError("Không tìm thấy danh mục.", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(data, "Lấy thông tin danh mục thành công.");
        }

        [HttpPost]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateAsync(dto);
            if (!result)
                return ResponseHelper.ResponseError("Tạo danh mục thất bại.", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<object>(null, "Tạo danh mục thành công.");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ResponseError("Dữ liệu không hợp lệ.", HttpStatusCode.BadRequest);

            var result = await _categoryService.UpdateAsync(id,dto);
            if (!result)
                return ResponseHelper.ResponseError("Không tìm thấy danh mục để cập nhật.", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess<object>(null, "Cập nhật danh mục thành công.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
                return ResponseHelper.ResponseError("Không tìm thấy danh mục để xóa.", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess<object>(null, "Xóa danh mục thành công.");
        }
    }
}
