namespace DATN.Dtos.ProductDto
{
    public class CreateProductWithFilesDto
    {

        public string NameProduct { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public List<IFormFile> Images { get; set; }
        public int MainImageIndex { get; set; }
    }
}
