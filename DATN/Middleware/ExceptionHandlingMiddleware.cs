using DATN.Dtos;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
namespace DATN.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                var responseDto = new ResponseDto<object>();
                responseDto.Data = null;

                switch (ex)
                {
                    case ValidationException ve:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        responseDto.Status = response.StatusCode;
                        responseDto.Message = "Lỗi xác thực: " + ve.Message;
                        break;

                    case UnauthorizedAccessException ue:
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        responseDto.Status = response.StatusCode;
                        responseDto.Message = "Không có quyền truy cập: " + ue.Message;
                        break;

                    case DbUpdateException dbEx:
                        response.StatusCode = (int)HttpStatusCode.Conflict;
                        responseDto.Status = response.StatusCode;
                        responseDto.Message = "Lỗi cơ sở dữ liệu: " + dbEx.InnerException?.Message ?? dbEx.Message;
                        break;

                    case KeyNotFoundException knf:
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        responseDto.Status = response.StatusCode;
                        responseDto.Message = "Không tìm thấy tài nguyên: " + knf.Message;
                        break;

                    default:
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        responseDto.Status = response.StatusCode;
                        responseDto.Message = "Lỗi server: " + ex.Message;
                        break;
                }

                _logger.LogError(ex, $"Đã xảy ra lỗi: {ex.Message}");

                var json = JsonSerializer.Serialize(responseDto);
                await response.WriteAsync(json);
            }
        }
    }
}
