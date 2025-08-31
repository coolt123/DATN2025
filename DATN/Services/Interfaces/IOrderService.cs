using DATN.Dtos.OrderDto;
using DATN.Entities;

namespace DATN.Services.Interfaces
{
    public interface IOrderService
    {
        Task<object> CreateOrderWithPaymentAsync(CreateOrderWithPaymentDto dto);

        // Cập nhật trạng thái đơn khi thanh toán thành công
        Task<bool> MarkOrderAsPaidAsync(int orderId);

        // Lấy danh sách đơn hàng tổng quát (Admin)
        Task<List<OrderItemDto>> GetAllOrdersAsync();
        Task<List<OrderItemDto>> GetOrdersByStatusAsync(string status);
        Task<List<OrderItemDto>> GetOrdersByUserIdAsync(string userId);
        Task<List<OrderItemDto>> SearchOrdersAsync(string keyword);

        // Lấy đơn hàng kèm chi tiết
        Task<OrderFullDto> GetOrderWithDetailsAsync(int orderId);
    }
}
