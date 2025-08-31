namespace DATN.Entities
{
    public class Coupon : Time
    {
        public int CouponId { get; set; }
        public string Code { get; set; }
        public int DiscountPercent { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
