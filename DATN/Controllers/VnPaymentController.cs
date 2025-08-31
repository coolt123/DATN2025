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
        private readonly IOrderService _orderService;

        public PaymentController(IVnPayService vnPayService, IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _orderService = orderService;
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
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            // Kiểm tra phản hồi từ VNPAY
            if (response == null || response.VnPayResponseCode != "00")
            {
                return Redirect($"/payment-fail?code={response?.VnPayResponseCode}");
            }

            // Lấy OrderId từ vnp_TxnRef
            if (int.TryParse(response.TxnRef, out int orderId))
            {
                var success = await _orderService.MarkOrderAsPaidAsync(orderId);

                if (success)
                    return Redirect($"/payment-success?orderId={orderId}");
                else
                    return Redirect("/payment-fail?reason=update-failed");
            }

            return Redirect("/payment-fail?reason=invalid-order-id");
        }
    }
}

