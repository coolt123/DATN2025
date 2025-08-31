namespace DATN.Dtos.OrderDto
{
    public class CreateOrderWithPaymentDto
    {
        public string IdUser { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }

        public List<CreateOrderDetailDto> OrderDetails { get; set; }
        public CreatePaymentDto Payment { get; set; }
    }
}
