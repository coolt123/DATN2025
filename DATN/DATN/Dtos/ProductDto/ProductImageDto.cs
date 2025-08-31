namespace DATN.Dtos.ProductDto
{
    public class ProductImageDto
    {
        public int ProductImageId { get; set; }
        public string ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMain { get; set; }
    }
}
