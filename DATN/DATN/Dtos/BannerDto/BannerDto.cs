namespace DATN.Dtos.BannerDto
{
    public class BannerDtos
    {
        public string Title { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public IFormFile Image { get; set; }
        public int? CategoryId { get; set; }
    }
}
