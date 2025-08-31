using DATN.Dtos.BannerDto;
using DATN.Entities;
using DATN.Exceptions;
using DATN.Helpers; // Giả định chứa ResponseHelper, AppException
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class BannerController : ControllerBase
    {
        private readonly IBannerService _service;

        public BannerController(IBannerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return ResponseHelper.ResponseSuccess(result, "Lấy danh sách banner thành công.");
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                throw new AppException("Không tìm thấy banner.", (int)HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(result, "Lấy thông tin banner thành công.");
        }

        [HttpPost]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Create([FromForm] BannerDtos dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return ResponseHelper.ResponseSuccess(result, "Tạo banner thành công.", statusCode: HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Update(int id, [FromForm] BannerDtos dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == null)
                throw new AppException("Không tìm thấy banner cần cập nhật.", (int)HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(result, "Cập nhật banner thành công.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                throw new AppException("Không tìm thấy banner cần xóa.", (int)HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess<String>(null, "Xóa banner thành công.");
        }
    }
}
