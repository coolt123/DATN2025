namespace DATN.Dtos.OrderDto
{
    public class OrderItemDto
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }         
        public string Email { get; set; }            
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string phonenumber { get; set; }
        public string shippingAddress { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
    }
}
