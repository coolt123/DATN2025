namespace DATN.Entities
{
    public class ProductView
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? UserId { get; set; }         
        public string? SessionId { get; set; }      
        public DateTime ViewedAt { get; set; }

        public Product Product { get; set; }
    }
}
