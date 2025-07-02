using DATN.Dtos;
using DATN.Dtos.CategoriesDto;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            return Ok(new ResponseDto<object>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Lấy danh sách danh mục thành công.",
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _categoryService.GetByIdAsync(id);
            if (data == null)
            {
                return NotFound(new ResponseDto<object>
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Message = "Không tìm thấy danh mục.",
                    Data = null
                });
            }

            return Ok(new ResponseDto<object>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Lấy thông tin danh mục thành công.",
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateAsync(dto);
            if (!result)
            {
                return BadRequest(new ResponseDto<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "Tạo danh mục thất bại.",
                    Data = null
                });
            }

            return Ok(new ResponseDto<object>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Tạo danh mục thành công.",
                Data = null
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (id != dto.CategoryId)
            {
                return BadRequest(new ResponseDto<object>
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Message = "ID không khớp với danh mục.",
                    Data = null
                });
            }

            var result = await _categoryService.UpdateAsync(dto);
            if (!result)
            {
                return NotFound(new ResponseDto<object>
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Message = "Không tìm thấy danh mục để cập nhật.",
                    Data = null
                });
            }

            return Ok(new ResponseDto<object>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Cập nhật danh mục thành công.",
                Data = null
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ResponseDto<object>
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Message = "Không tìm thấy danh mục để xóa.",
                    Data = null
                });
            }

            return Ok(new ResponseDto<object>
            {
                Status = (int)HttpStatusCode.OK,
                Message = "Xóa danh mục thành công.",
                Data = null
            });
        }
    }
}
