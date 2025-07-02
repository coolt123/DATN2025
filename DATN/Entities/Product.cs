namespace DATN.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string NameProduct { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public Category Category { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<InventoryLog> InventoryLogs { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; }
    }
}
