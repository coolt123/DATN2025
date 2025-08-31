namespace DATN.Dtos.OrderDto
{
    public class CreatePaymentDto
    {
        public string PaymentMethod { get; set; }  // COD, VNPAY, Momo, etc.
        public string Status { get; set; }         // Pending, Paid, Failed
        public DateTime? PaidAt { get; set; }
    }
}
