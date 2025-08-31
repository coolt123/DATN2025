using DATN.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DATN.Helpers
{
    public static class ResponseHelper
    {
        public static IActionResult ResponseSuccess<T>(T data, string message = "Thành công", object? meta = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ObjectResult(new ResponseDto<T>
            {
                statusCode = (int)statusCode,
                Message = message,
                Data = data,
                Meta=meta
            })
            {
                StatusCode = (int)statusCode
            };
        }

        public static IActionResult ResponseError(string message, object? meta = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ObjectResult(new ResponseDto<object>
            {
                statusCode = (int)statusCode,
                Message = message,
                Data = null,
                Meta=meta
            })
            {
                StatusCode = (int)statusCode
            };
        }
    }
}
