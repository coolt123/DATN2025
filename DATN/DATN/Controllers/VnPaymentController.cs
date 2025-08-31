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

            return Redirect(url);

        }
        [HttpGet("test-vnpay")]

        public IActionResult TestVnpay()
        {
            var model = new PaymentInformationModel
            {
                Amount = 10000,
                OrderDescription = "Test thanh toán",
                OrderType = "other"
            };
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { paymentUrl = url });
        }

       
    }
}

