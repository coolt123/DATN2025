using DATN.Dtos.OrderDto;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/Orders
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderWithPaymentDto dto)
        {
            var result = await _orderService.CreateOrderWithPaymentAsync(dto);
            return ResponseHelper.ResponseSuccess(result, "Tạo đơn hàng thành công.");
        }

        // GET: api/Orders/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return ResponseHelper.ResponseSuccess(result, "Lấy tất cả đơn hàng thành công.");
        }

        // GET: api/Orders/status?value=Pending
        [HttpGet("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByStatus([FromQuery] string value)
        {
            var result = await _orderService.GetOrdersByStatusAsync(value);
            return ResponseHelper.ResponseSuccess(result, $"Lọc đơn hàng theo trạng thái: {value}.");
        }

        // GET: api/Orders/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var result = await _orderService.GetOrdersByUserIdAsync(userId);
            return ResponseHelper.ResponseSuccess(result, "Lấy đơn hàng theo người dùng.");
        }

        // GET: api/Orders/{orderId}
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            var result = await _orderService.GetOrderWithDetailsAsync(orderId);
            if (result == null)
                return ResponseHelper.ResponseError("Không tìm thấy đơn hàng.", System.Net.HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(result, "Lấy chi tiết đơn hàng thành công.");
        }

        // GET: api/Orders/search?keyword=abc
        [HttpGet("search")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _orderService.SearchOrdersAsync(keyword);
            return ResponseHelper.ResponseSuccess(result, $"Tìm kiếm đơn hàng theo từ khoá: {keyword}.");
        }
    }
}
