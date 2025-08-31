namespace DATN.Dtos.ProductDto
{
    public class UpdateProductWithFilesDto
    {
        public string NameProduct { get; set; } 
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public int MaterialId { get; set; }
        public int StyleId { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? PromotionStart { get; set; }
        public DateTime? PromotionEnd { get; set; }
        public int MainImageIndex { get; set; } = -1;
        public List<IFormFile>? NewImages { get; set;}
        public List<string>? ImageUrls { get; set; }
        public List<UpdateProductDescriptionDetailDto>? DescriptionDetails { get; set; }

    }
    public class UpdateProductDescriptionDetailDto
    {
        public int? IdDescription { get; set; } 
        public string Title { get; set; }
        public string Detail { get; set; }

      
        public string? ExistingImageUrl { get; set; }

        public IFormFile? NewImage { get; set; }
    }
}