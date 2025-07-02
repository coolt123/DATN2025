using Identity.Entities;

namespace DATN.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public string IdUser { get; set; }
        
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<Payment> Payments { get; set; }
    }
}
