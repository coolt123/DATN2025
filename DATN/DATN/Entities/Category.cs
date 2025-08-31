namespace DATN.Entities
{
    public class Category : Time
    {
        public int CategoryId { get; set; }
        public string NameCategory { get; set; }
        public ICollection<Product> Products { get; set; }
        public ICollection<Banner> Banners { get; set; }
    }
}
