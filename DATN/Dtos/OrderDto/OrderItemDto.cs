namespace DATN.Dtos.OrderDto
{
    public class OrderItemDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }         // Tên người đặt
        public string Email { get; set; }            // Email người đặt
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
