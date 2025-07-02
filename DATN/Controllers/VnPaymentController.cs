using Microsoft.AspNetCore.Mvc;
using DATN.Dtos.Vnpay;
using DATN.Services.Interfaces;

namespace DATN.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;

        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost("create-vnpay-url")]
        public IActionResult CreatePaymentUrlVnpay([FromBody] PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            // Cách 1: Redirect luôn (trình duyệt sẽ chuyển trang)
            // return Redirect(url);

            // Cách 2: Trả về URL để frontend xử lý redirect
            return Ok(new { paymentUrl = url });
        }

        [HttpGet("vnpay-callback")]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Ok(response);
        }
    }
}
