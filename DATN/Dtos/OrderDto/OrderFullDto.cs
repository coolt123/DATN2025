namespace DATN.Dtos.OrderDto
{
    public class OrderFullDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }

        // Thông tin người đặt
        public string UserName { get; set; }
        public string Email { get; set; }

        // Thông tin đơn hàng
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }

        public List<OrderDetailItemDto> OrderDetails { get; set; }
    }

    public class OrderDetailItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }       // Lấy từ Product
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal => Quantity * UnitPrice;
    }
}
