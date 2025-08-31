using DATN.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace DATN.Filters
{
    public class ResponseWrapperFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
           
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                
                if (objectResult.Value is ResponseDto<object> || objectResult.Value?.GetType().Name.StartsWith("ResponseDto") == true)
                    return;

                var statusCode = objectResult.StatusCode ?? (int)HttpStatusCode.OK;

                var wrapped = new ResponseDto<object>
                {
                    Status = statusCode,
                    Message = "Success",
                    Data = objectResult.Value
                };

                context.Result = new ObjectResult(wrapped)
                {
                    StatusCode = statusCode
                };
            }
            else if (context.Result is EmptyResult)
            {
                context.Result = new ObjectResult(new ResponseDto<object>
                {
                    Status = (int)HttpStatusCode.NoContent,
                    Message = "No content",
                    Data = null
                })
                {
                    StatusCode = (int)HttpStatusCode.NoContent
                };
            }
        }
    }
}
