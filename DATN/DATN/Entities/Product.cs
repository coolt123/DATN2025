using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Entities
{
    public class Product : Time
    {
        public int ProductId { get; set; }
        public string NameProduct { get; set; }
        public string Description { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? PromotionStart { get; set; }
        public DateTime? PromotionEnd { get; set; }
        public int StyleId { get; set; }
        public int MaterialId { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public Category Category { get; set; }
        public Style Style { get; set; }
        public Material Material { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<InventoryLog> InventoryLogs { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; }
        public ICollection<ProductDescriptionDetail> DescriptionDetails { get; set; }

        public decimal CurrentPrice
        {
            get
            {
                if (SalePrice.HasValue
                    && PromotionStart <= DateTime.Now
                    && PromotionEnd >= DateTime.Now)
                {
                    return SalePrice.Value;
                }
                return Price;
            }
        }

        public bool IsOnSale
        {
            get
            {
                return SalePrice.HasValue
                       && SalePrice.Value < Price
                       && PromotionStart <= DateTime.Now
                       && PromotionEnd >= DateTime.Now;
            }
        }
        [NotMapped]
        public Dictionary<string, int> CachedVector { get; set; }
    }
}
