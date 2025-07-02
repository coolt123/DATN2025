namespace DATN.Entities
{
    public class Banner
    {
        public int BannerId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
