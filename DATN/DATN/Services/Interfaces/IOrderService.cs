using DATN.Dtos.OrderDto;
using DATN.Dtos.Vnpay;
using DATN.Entities;
using static DATN.Services.OrderService;
using static IQueryableExtensions;

namespace DATN.Services.Interfaces
{
    public interface IOrderService
    {
        Task<object> CreateOrderWithPaymentAsync(CreateOrderWithPaymentDto dto);

        Task<bool> UpdateOrder(int orderId);

        Task<PagedResult<OrderItemDto>> GetAllOrdersAsync(OrderQuery query);
        Task<List<OrderItemDto>> GetOrdersByStatusAsync(string status);
        Task<List<OrderDetailUser>> GetOrdersByUserIdAsync(string userId);

        Task<bool> ProcessVnPayCallbackAsync(PaymentResponseModel response);
        Task<List<OrderItemDto>> SearchOrdersAsync(string keyword);

        Task<OrderFullDto> GetOrderWithDetailsAsync(int orderId);
    }
}
