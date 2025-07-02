using Identity.Entities;

namespace DATN.Entities
{
    public class Cart
    {
        public int CartId { get; set; }
        public string IdUser { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public ApplicationUser User { get; set; }
        public Product Product { get; set; }
    }
}
