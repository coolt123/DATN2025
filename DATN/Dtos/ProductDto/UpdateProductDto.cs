namespace DATN.Dtos.ProductDto
{
    public class UpdateProductDto
    {
        public int ProductId { get; set; }
        public string NameProduct { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
    }
}
