using DATN.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DATN.Helpers
{
    public static class ResponseHelper
    {
        public static IActionResult ResponseSuccess<T>(T data, string message = "Thành công", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ObjectResult(new ResponseDto<T>
            {
                Status = (int)statusCode,
                Message = message,
                Data = data
            })
            {
                StatusCode = (int)statusCode
            };
        }

        public static IActionResult ResponseError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ObjectResult(new ResponseDto<object>
            {
                Status = (int)statusCode,
                Message = message,
                Data = null
            })
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
