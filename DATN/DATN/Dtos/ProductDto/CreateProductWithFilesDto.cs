using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos.ProductDto
{
    public class CreateProductDescriptionDetailDto
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public IFormFile? ImageDesdetail { get; set; }
    }
    public class CreateProductWithFilesDto
    {
        [Required]
        public string NameProduct { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int StockQuantity { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public List<IFormFile> Images { get; set; }
     
        public decimal? salePrice { get; set; }
        public DateTime? promotionStart { get; set; }
        public DateTime? promotionEnd {  get; set; }
        public DateTime? CreatedDate { get; set; }
        public int MainImageIndex { get; set; }
        [Required]
        public int MaterialId { get; set; }
        [Required]
        public int StyleId { get; set; }
        public List<CreateProductDescriptionDetailDto>? DescriptionDetails { get; set; }

        public DateTime CreateAt { get; set; }

    }
}
