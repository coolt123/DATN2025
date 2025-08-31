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
        public string Style { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? PromotionStart { get; set; }
        public DateTime? PromotionEnd { get; set; }
        public int Styleid { get; set; }
        public string Material { get; set; }
        public int Materialid { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CategoryName { get; set; }
        public decimal CurrentPrice { get; set; }
        public bool IsOnSale { get; set; }
        public int SoldQuantity { get; set; } = 0;      
        public int RemainingQuantity { get; set; }
        public List<ProductImageDto> Images { get; set; }
        public List<ProductDescriptionDetailDto> DescriptionDetails { get; set; } 
    }
    public class ProductDescriptionDetailDto
    {
        public int IdDescription { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string ImageUrl { get; set; }
    }
}
