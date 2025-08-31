
using DATN.Dtos.StyleDto;
using DATN.Entities;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DATN.Controllers.Admin
{
    [Route("api/styles")]
    [ApiController]
    [Authorize(Roles = AppRole.Admin)]
    public class StyleController : ControllerBase
    {
        private readonly IStyleService _styleService;

        public StyleController(IStyleService styleService)
        {
            _styleService = styleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _styleService.GetAllAsync();
            return ResponseHelper.ResponseSuccess(result, "Lấy danh sách thành công");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _styleService.GetByIdAsync(id);
            if (result == null)
                return ResponseHelper.ResponseError("Không tìm thấy style", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(result, "Lấy style thành công");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStyleDto dto)
        {
            var result = await _styleService.CreateAsync(dto);
            if (!result)
                return ResponseHelper.ResponseError("Tạo style thất bại", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<object>(null, "Tạo style thành công");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStyleDto dto)
        {
            var result = await _styleService.UpdateAsync(id, dto);
            if (!result)
                return ResponseHelper.ResponseError("Cập nhật style thất bại", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<object>(null, "Cập nhật style thành công");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _styleService.DeleteAsync(id);
            if (!result)
                return ResponseHelper.ResponseError("Xoá style thất bại", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<object>(null, "Xoá style thành công");
        }
    }
}
