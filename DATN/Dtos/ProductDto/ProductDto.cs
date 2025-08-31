namespace DATN.Dtos.ProductDto
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string NameProduct { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string Style { get; set; }
        public string Material { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CategoryName { get; set; }
        public List<ProductImageDto> Images { get; set; }
    }
}
