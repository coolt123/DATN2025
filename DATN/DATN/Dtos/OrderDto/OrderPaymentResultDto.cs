namespace DATN.Dtos.OrderDto
{
    public class OrderPaymentResultDto
    {
        public bool Success { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
