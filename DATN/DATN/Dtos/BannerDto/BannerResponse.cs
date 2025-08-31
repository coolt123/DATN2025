namespace DATN.Dtos.BannerDto.BannerDto
{
    public class BannerResponse
    {
        public int BannerId { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public string? LinkUrl { get; set; }
        public string? CategoryName { get; set; }
    }
}
