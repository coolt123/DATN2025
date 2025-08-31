using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace DATN.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IOrderService _orderService;

        public CheckoutController(IVnPayService vnPayService, IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _orderService = orderService;
        }
        [HttpGet("PaymentCallbackVnpay")]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = await _vnPayService.PaymentExecute(Request.Query);
            if(response.Success)
            {
                return Redirect($"http://localhost:3000/checkoutresult?status=success&orderId={response.OrderId}");
            }
            else
            {
                return Redirect($"http://localhost:3000/checkoutresult?status=fail&orderId={response.OrderId}");
            }
        }

    }
}
