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
using static IQueryableExtensions;

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
        private async Task SendOrderConfirmationEmailAsync(Order order)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == order.IdUser);
            if (user == null) return;

            string subject = $"[Đơn hàng #{order.OrderCode}] Xác nhận đặt hàng thành công";
            string body = $"Xin chào {user.FullName ?? user.UserName},\n\n" +
                          $"Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi!\n\n" +
                          $"- Mã đơn hàng: {order.OrderCode}\n" +
                          $"- Phương thức thanh toán: {order.Payments.FirstOrDefault()?.PaymentMethod}\n" +
                          $"- Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}\n" +
                          $"- Tổng tiền: {order.TotalAmount:N0} đ\n" +
                          $"- Địa chỉ nhận: {order.ShippingAddress}\n\n" +
                          $"Chúng tôi sẽ xử lý đơn hàng của bạn sớm nhất.\n\nTrân trọng.";

            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        public async Task<object> CreateOrderWithPaymentAsync(CreateOrderWithPaymentDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
               
                string orderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";

               
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
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);

                    if (product == null || product.deleteflag)
                        throw new Exception($"Sản phẩm {detail.ProductId} không tồn tại.");

                    if (product.StockQuantity < detail.Quantity)
                        throw new Exception($"Sản phẩm {product.NameProduct} không đủ số lượng tồn kho.");

                    product.StockQuantity -= detail.Quantity;
                    _context.Products.Update(product);
                    _context.InventoryLogs.Add(new InventoryLog
                    {
                        ProductId = product.ProductId,
                        LogType = "Sale",
                        LogDate = DateTime.UtcNow,
                        Note = $"Bán {detail.Quantity} sản phẩm {product.NameProduct}, tồn kho còn {product.StockQuantity}"
                    });
                }
                await _context.SaveChangesAsync();
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = dto.Payment.PaymentMethod,
                    PaymentStatus = dto.Payment.PaymentMethod == "VNPAY" ? "Pending" : dto.Payment.Status ?? "Paid",
                    PaymentDate = dto.Payment.PaidAt ?? DateTime.UtcNow
                };
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

               
                string redirectUrl = null;
                if (dto.Payment.PaymentMethod == "VNPAY")
                {
                    var httpContext = _httpContextAccessor.HttpContext;
                    if(httpContext == null)
                        throw new Exception("HttpContext không khả dụng để tạo VNPAY URL.");

                    var paymentModel = new PaymentInformationModel
                    {   
                        OrderId=order.OrderId,
                        OrderType = "other",
                        Amount = order.TotalAmount,
                        OrderDescription = $"Thanh toán đơn hàng {order.OrderCode}",
                        Name = $"Khách hàng {dto.IdUser}"
                    };

                    redirectUrl = _vnPayService.CreatePaymentUrl(paymentModel, httpContext);
                }
                else 
                {
                    await SendOrderConfirmationEmailAsync(order);
                }
                

                await transaction.CommitAsync();

                return new OrderPaymentResultDto
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
        public async Task<bool> ProcessVnPayCallbackAsync(PaymentResponseModel response)
        {
            if (response.VnPayResponseCode != "00")
                return false; 
            var order = await _context.Orders
                                .Include(o => o.Payments)
                                .Include(o => o.User)
                                .FirstOrDefaultAsync(o => o.OrderCode == response.OrderId);

            if (order == null) return false;

            order.Status = "Processing";
            var payment = order.Payments.FirstOrDefault();
            if (payment != null)
            {
                payment.PaymentStatus = "Paid";
                payment.PaymentDate = DateTime.UtcNow;
            };

            await _context.SaveChangesAsync();
            await SendOrderConfirmationEmailAsync(order);

            return true;
        }
        public class OrderQuery
        {
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string? Search { get; set; }
        }
        public async Task<PagedResult<OrderItemDto>> GetAllOrdersAsync(OrderQuery query)
        {
            var q = _context.Orders
               .Include(o => o.User)
               .Include(o => o.Payments)
               .OrderByDescending(o => o.OrderDate)
               .AsQueryable();
            if (!string.IsNullOrEmpty(query.Search))
            {
                q = q.Where(o => o.OrderCode.Contains(query.Search));
            }

            var paged = await q.ToPagedResultAsync(
            query.Page,
            query.PageSize,
            items => _mapper.Map<List<OrderItemDto>>(items));
            return paged;
        }

        public async Task<List<OrderItemDto>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _context.Orders      
               .Include(o => o.User)
               .Where(o => o.Status.ToLower() == status.ToLower())
               .OrderByDescending(o => o.OrderDate)
                .Include(o => o.Payments)
               .ToListAsync();

            return _mapper.Map<List<OrderItemDto>>(orders);
        }

        public async Task<List<OrderDetailUser>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.IdUser == userId)
                .OrderByDescending(o => o.OrderDate)
                .Include(o=>o.Payments)
                .ToListAsync();

            return _mapper.Map<List<OrderDetailUser>>(orders);
        }

        public async Task<OrderFullDto> GetOrderWithDetailsAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payments)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            return _mapper.Map<OrderFullDto>(order);
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

        public async Task<bool> UpdateOrder(int orderId)
        {
            var order = await _context.Orders.Include(o=>o.Payments).FirstOrDefaultAsync(o=>o.OrderId == orderId); 
            if (order == null)
                return false;
            var payment = order.Payments.FirstOrDefault();
            if (payment != null)
            {
                payment.PaymentStatus = "paid";
            }
            order.Status = "Complete";
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
