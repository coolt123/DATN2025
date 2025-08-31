namespace DATN.Dtos.OrderDto
{
    public class OrderDetailUser
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string phonenumber { get; set; }
        public string userName { get; set; }
        public string PaymentStatus { get;set; }
    }
}
