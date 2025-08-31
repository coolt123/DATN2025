
using DATN.Dtos.MaterialDto;
using DATN.Entities;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRole.Admin)]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialsController(IMaterialService materialService)
        {
            _materialService = materialService;
        }

        [HttpPost]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateMaterialDto dto)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ResponseError("Dữ liệu không hợp lệ.", HttpStatusCode.BadRequest);

            var result = await _materialService.CreateAsync(dto);
            if (result == null)
                return ResponseHelper.ResponseError("Tạo chất liệu thất bại.", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<object>(null, "Tạo chất liệu thành công.");
        }

        [HttpGet]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetAll()
        {
            var materials = await _materialService.GetAllAsync();
            return ResponseHelper.ResponseSuccess(materials, "Lấy danh sách chất liệu thành công.");
        }

        [HttpGet("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetById(int id)
        {
            var material = await _materialService.GetByIdAsync(id);
            if (material == null)
                return ResponseHelper.ResponseError("Không tìm thấy chất liệu.", HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(material, "Lấy thông tin chất liệu thành công.");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMaterialDto dto)
        {
            if (!ModelState.IsValid)
                return ResponseHelper.ResponseError("Dữ liệu không hợp lệ.", HttpStatusCode.BadRequest);

            var success = await _materialService.UpdateAsync(id, dto);
            if (!success)
                return ResponseHelper.ResponseError("Cập nhật chất liệu thất bại.", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<object>(null, "Cập nhật chất liệu thành công.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _materialService.DeleteAsync(id);
            if (!success)
                return ResponseHelper.ResponseError("Xoá chất liệu thất bại.", HttpStatusCode.BadRequest);

            return ResponseHelper.ResponseSuccess<object>(null, "Xoá chất liệu thành công.");
        }
    }
}
