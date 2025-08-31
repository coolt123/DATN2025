using Identity.Entities;

namespace DATN.Entities
{
    public class Review : Time
    {
        public int ReviewId { get; set; }
        public string IdUser { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public ApplicationUser User { get; set; }
        public Product Product { get; set; }
    }
}
