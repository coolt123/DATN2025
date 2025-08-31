using AutoMapper;
using DATN.DbContexts;
using DATN.Dtos.OrderDto;
using DATN.Dtos.Vnpay;
using DATN.Entities;
using DATN.Services.Interfaces;
using DATN.Services.ResetPass;
using EmailSender;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Services
{
    public class OrderService : IOrderService
    {
        private readonly Data _context;
        private readonly IMapper _mapper;
        private readonly IVnPayService _vnPayService;
        private readonly IEmailSender _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(
            Data context,
            IMapper mapper,
            IVnPayService vnPayService,
            IEmailSender emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _vnPayService = vnPayService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<object> CreateOrderWithPaymentAsync(CreateOrderWithPaymentDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Tạo mã đơn hàng
                string orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

                // 2. Tạo đơn hàng
                var order = new Order
                {
                    OrderCode = orderCode,
                    IdUser = dto.IdUser,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = dto.ShippingAddress,
                    TotalAmount = dto.TotalAmount,
                    Status = "Pending",
                    OrderDetails = dto.OrderDetails.Select(d => new OrderDetail
                    {
                        ProductId = d.ProductId,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice
                    }).ToList()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 3. Tạo payment record
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = dto.Payment.PaymentMethod,
                    PaymentStatus = dto.Payment.Status ?? "Pending",
                    PaymentDate = dto.Payment.PaidAt ?? DateTime.UtcNow
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // 4. Tạo link thanh toán (nếu cần)
                string redirectUrl = null;
                if (dto.Payment.PaymentMethod == "VNPAY")
                {
                    var httpContext = _httpContextAccessor.HttpContext;

                    var paymentModel = new PaymentInformationModel
                    {
                        OrderType = "billpayment",
                        Amount = order.TotalAmount,
                        OrderDescription = $"Thanh toán đơn hàng {order.OrderCode}",
                        Name = $"Khách hàng {dto.IdUser}"
                    };

                    redirectUrl = _vnPayService.CreatePaymentUrl(paymentModel, httpContext);
                }

                // 5. Gửi email xác nhận
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.IdUser);
                if (user != null)
                {
                    string subject = $"[Đơn hàng #{order.OrderCode}] Xác nhận đặt hàng thành công";
                    string body = $"Xin chào {user.FullName ?? user.UserName},\n\n" +
                                  $"Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi!\n\n" +
                                  $"- Mã đơn hàng: {order.OrderCode}\n" +
                                  $"- Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}\n" +
                                  $"- Tổng tiền: {order.TotalAmount:N0} đ\n" +
                                  $"- Địa chỉ nhận: {order.ShippingAddress}\n\n" +
                                  $"Chúng tôi sẽ xử lý đơn hàng của bạn sớm nhất.\n\nTrân trọng.";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }

                await transaction.CommitAsync();

                return new
                {
                    Success = true,
                    OrderId = order.OrderId,
                    OrderCode = order.OrderCode,
                    Message = "Đặt hàng thành công",
                    PaymentUrl = redirectUrl
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new
                {
                    Success = false,
                    Message = $"Lỗi khi đặt hàng: {ex.Message}"
                };
            }
        }

        public async Task<List<OrderItemDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return _mapper.Map<List<OrderItemDto>>(orders);
        }

        public async Task<List<OrderItemDto>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _context.Orders
               .Include(o => o.User)
               .Where(o => o.Status.ToLower() == status.ToLower())
               .OrderByDescending(o => o.OrderDate)
               .ToListAsync();

            return _mapper.Map<List<OrderItemDto>>(orders);
        }

        public async Task<List<OrderItemDto>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.IdUser == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return _mapper.Map<List<OrderItemDto>>(orders);
        }

        public async Task<OrderFullDto> GetOrderWithDetailsAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            return _mapper.Map<OrderFullDto>(order);
        }

        public Task<bool> MarkOrderAsPaidAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<OrderItemDto>> SearchOrdersAsync(string keyword)
        {
            keyword = keyword.ToLower().Trim();
            var orders = await _context.Orders
                .Include(o => o.User)
                .Where(o =>
                    o.User.Email.ToLower().Contains(keyword) ||
                    o.User.UserName.ToLower().Contains(keyword) ||
                    o.OrderCode.ToLower().Contains(keyword)
                )
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return _mapper.Map<List<OrderItemDto>>(orders);
        }

    }
}
