namespace DATN.Dtos.CartDto
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
  