using DATN.Dtos.OrderDto;
using DATN.Entities;
using DATN.Helpers;
using DATN.Services.Implements;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using static DATN.Services.OrderService;

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

        
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderWithPaymentDto dto)
        {
            try
            {
                var result = await _orderService.CreateOrderWithPaymentAsync(dto);
                return ResponseHelper.ResponseSuccess(result, "Tạo đơn hàng thành công.");
            }
           catch (Exception ex) {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }
        [HttpPut("update")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> UpdateOrder(int orderid)
        {
            try
            {
                var result = await _orderService.UpdateOrder(orderid);
                return ResponseHelper.ResponseSuccess(result, "cập nhật thành công");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        
        [HttpGet("all")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderQuery query)
        {
            try
            {
                var result = await _orderService.GetAllOrdersAsync(query);
                return ResponseHelper.ResponseSuccess(data: result.Items,
                message: "Lấy danh sách đơn hàng thành công",
                meta: result.Meta);
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        
        [HttpGet("status")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetByStatus([FromQuery] string value)
        {
            try
            {
                var result = await _orderService.GetOrdersByStatusAsync(value);
                return ResponseHelper.ResponseSuccess(result, $"Lọc đơn hàng theo trạng thái: {value}.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

       
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetByUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                    return Unauthorized();
                var result = await _orderService.GetOrdersByUserIdAsync(userId);
                return ResponseHelper.ResponseSuccess(result, "Lấy đơn hàng theo người dùng.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }

        
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            var result = await _orderService.GetOrderWithDetailsAsync(orderId);
            if (result == null)
                return ResponseHelper.ResponseError("Không tìm thấy đơn hàng.", System.Net.HttpStatusCode.NotFound);

            return ResponseHelper.ResponseSuccess(result, "Lấy chi tiết đơn hàng thành công.");
        }

       
        [HttpGet("search")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            try
            {
                var result = await _orderService.SearchOrdersAsync(keyword);
                return ResponseHelper.ResponseSuccess(result, $"Tìm kiếm đơn hàng theo từ khoá: {keyword}.");
            }
            catch (Exception ex)
            {
                return ResponseHelper.ResponseError(ex.Message);
            }
        }
    }
}
