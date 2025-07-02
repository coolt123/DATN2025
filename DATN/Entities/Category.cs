namespace DATN.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string NameCategory { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
